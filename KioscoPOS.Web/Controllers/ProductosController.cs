using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private const int PageSize = 10;
    private static readonly string[] ExtensionesPermitidas ={".jpg", ".jpeg", ".png"};
    private const long TamanoMaximo = 2*1024*1024;

    public ProductosController(ApplicationDbContext context , IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

//listado de busqueda , filtrado de stock y paginado
public async Task<IActionResult> Index(string? buscar,string? estado, int page = 1)
    {
    page = Math.Max(page, 1);
    var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

    if(!string.IsNullOrWhiteSpace(buscar))
        query = query.Where(p => p.Nombre.Contains(buscar));

    if(estado =="bajo")
        query = query.Where (p => p.StockActual <= p.StockMinimo);
    else if(estado == "sin")
        query = query.Where(p => p.StockActual == 0);

    var total = await query.CountAsync();
    page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
    var items = await query
        .OrderBy(p => p.Nombre)
        .Skip((page - 1)* PageSize)
        .Take(PageSize)
        .ToListAsync();
        ViewBag.Buscar = buscar;
        ViewBag.Estado = estado;
        return View (new PagedResult<Producto>
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalItems = total
        });
    }

//alta
[Authorize(Roles ="Administrador")]
public IActionResult Create() => View(new ProductoViewModel());

[HttpPost , ValidateAntiForgeryToken]
[Authorize(Roles = "Administrador")]
public async Task<IActionResult> Create(ProductoViewModel vm)
    {
        await ValidarAsync(vm);
        if(!ModelState.IsValid)
        return await VolverAlFormulario(vm);

        var producto = new Producto
        {
            Nombre = vm.Nombre,
            PrecioCosto = vm.PrecioCosto,
            PrecioVenta = vm.PrecioVenta,
            StockActual = vm.StockActual,
            StockMinimo = vm.StockMinimo,
            CategoriaId = vm.CategoriaId,
            
            ImagenPath = await GuardarImagenAsync(vm.ImagenArchivo)
        };
        _context.Add(producto);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Producto creado";
        return RedirectToAction(nameof(Index));
    }
//modificacion

[Authorize(Roles ="Administrador")]
public async Task<IActionResult> Edit(int id)
    {
        var p = await _context.Productos.Include(x => x.Categoria).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null ) return NotFound();
        return View (new ProductoViewModel
        {
            Id = p.Id,
            Nombre = p.Nombre,
            PrecioCosto = p.PrecioCosto,
            PrecioVenta = p.PrecioVenta,
            StockMinimo = p.StockMinimo,
            CategoriaId = p.CategoriaId,
            CategoriaNombre = p.Categoria?.Nombre,
            ImagenPath = p.ImagenPath
        });
    }

    [HttpPost , ValidateAntiForgeryToken]
    [Authorize(Roles ="Administrador")]
    public async Task<IActionResult> Edit(int id , ProductoViewModel vm)
    {
        if(id !=vm.Id) return BadRequest();
        var producto = await _context.Productos.FindAsync(id);
        if(producto == null) return NotFound();
        await ValidarAsync(vm);

        if (!ModelState.IsValid)
        {
            vm.ImagenPath = producto.ImagenPath;
            return await VolverAlFormulario(vm);
        }
        producto.Nombre = vm.Nombre;
        producto.PrecioCosto = vm.PrecioCosto;
        producto.PrecioVenta = vm.PrecioVenta;
        producto.StockMinimo = vm.StockMinimo;
        producto.CategoriaId = vm.CategoriaId;
        if(vm.ImagenArchivo != null)
        {
            BorrarImagen(producto.ImagenPath);
            producto.ImagenPath = await GuardarImagenAsync(vm.ImagenArchivo);
        }
        await _context.SaveChangesAsync();
        TempData["Ok"]= "Producto actualizado";
        return RedirectToAction(nameof(Index));
    }

    //baja para los admins

    [HttpPost,ValidateAntiForgeryToken]
    [Authorize(Roles ="Administrador")]
    public async Task<IActionResult>Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        var tieneHistorial =
        await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id )||
        await _context.DetalleCompras.AnyAsync(d => d.ProductoId == id)||
        await _context.MovimientosStock.AnyAsync(m => m.ProductoId ==id);
        if (tieneHistorial)
        {
            TempData["Error"] = "No se puede eliminar el producto: tiene movimientos registrados.";
            return RedirectToAction(nameof(Index));
        }
        BorrarImagen(producto.ImagenPath);
        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        TempData["Ok"]="Producto eliminado";
        return RedirectToAction(nameof(Index));
    }

    //  variables auxiliares

    private async Task ValidarAsync(ProductoViewModel vm)
    {
        if (!await _context.Categorias.AnyAsync(c => c.Id == vm.CategoriaId))
            ModelState.AddModelError("CategoriaId", "La categoría no existe");

        if (vm.PrecioVenta < vm.PrecioCosto)
            ModelState.AddModelError("PrecioVenta", "El precio de venta es menor al costo");

        if (vm.ImagenArchivo != null)
        {
            var ext = Path.GetExtension(vm.ImagenArchivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(ext))
                ModelState.AddModelError("ImagenArchivo", "Formato no permitido (jpg, jpeg o png)");
            else if (vm.ImagenArchivo.Length > TamanoMaximo)
                ModelState.AddModelError("ImagenArchivo", "La imagen supera los 2 MB");
        }
    }

    private async Task<IActionResult> VolverAlFormulario(ProductoViewModel vm)
    {
        vm.CategoriaNombre = await _context.Categorias
            .Where(c => c.Id == vm.CategoriaId)
            .Select(c => c.Nombre)
            .FirstOrDefaultAsync();
        return View(vm.Id == 0 ? "Create" : "Edit", vm);
    }

    private async Task<string?> GuardarImagenAsync(IFormFile? archivo)
    {
        if (archivo == null || archivo.Length == 0) return null;

        var carpeta = Path.Combine(_env.WebRootPath, "uploads", "productos");
        Directory.CreateDirectory(carpeta);

        var nombre = Guid.NewGuid() + Path.GetExtension(archivo.FileName).ToLowerInvariant();
        using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
        await archivo.CopyToAsync(stream);

        return "/uploads/productos/" + nombre;
    }

    private void BorrarImagen(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        var fisico = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
        if (System.IO.File.Exists(fisico)) System.IO.File.Delete(fisico);
    }
}