using System.Globalization;
using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Services;
using KioscoPOS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = Roles.Administrador)]
public class ComprasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IComprasService _compraService;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int PageSize = 10;

    public ComprasController(ApplicationDbContext context, IComprasService compraService,
    UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _compraService = compraService;
        _userManager = userManager;
    }
    // paginado
    public async Task<IActionResult> Index(string? buscar, EstadoCompra? estado,
            DateTime? desde, DateTime? hasta, int page = 1)
    {
        page = Math.Max(page, 1);

        var query = _context.Compras.Include(c => c.Proveedor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(c => c.Proveedor.Nombre.Contains(buscar.Trim()));
        if (estado.HasValue)
            query = query.Where(c => c.Estado == estado);
        if (desde.HasValue)
            query = query.Where(c => c.Fecha >= desde.Value.Date);
        if (hasta.HasValue)
            query = query.Where(c => c.Fecha < hasta.Value.Date.AddDays(1));

        var total = await query.CountAsync();
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));

        var items = await query
            .OrderByDescending(c => c.Fecha).ThenByDescending(c => c.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Buscar = buscar;
        ViewBag.Estado = estado;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

        return View(new PagedResult<Compra>
        {
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalItems = total
        });
    }

    public IActionResult Create() => View(new CompraCrearVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompraCrearVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var id = await _compraService.CrearAsync(vm.ProveedorId, vm.Fecha, _userManager.GetUserId(User)!);
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (StockException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }

    // aca se armar el borrador que se confirma o se anula 
    public async Task<IActionResult> Detalle(int id)
    {
        var compra = await _context.Compras
        .Include(c => c.Proveedor)
        .Include(c => c.Usuario)
        .Include(c => c.AnuladaPor)
        .Include(c => c.Detalles).ThenInclude(d => d.Producto)
        .FirstOrDefaultAsync(c => c.Id == id);
        if (compra == null) return NotFound();
        return View(compra);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> AgregarItem(int id, int productoId, int cantidad, string costoUnitario)
    {
        if (!decimal.TryParse(costoUnitario, NumberStyles.Number, CultureInfo.InvariantCulture, out var costo))
        {
            TempData["Error"] = "costo invalido";
            return Task.FromResult<IActionResult>(RedirectToAction(nameof(Detalle), new { id }));
        }
        return Ejecutar(id, () => _compraService.AgregarItemsAsync(id, productoId, cantidad, costo), "item agregado");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> QuitarItems(int id, int detalleID) => Ejecutar(id, () => _compraService.QuitarItemsAsync(id, detalleID), "item agregado");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Confirmar(int id) =>
    Ejecutar(id, () => _compraService.ConfirmarAsync(id, _userManager.GetUserId(User)!),
    "compra confirmada : stock y costo actualizados");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Anular(int id, string motivo) =>
        Ejecutar(id, () => _compraService.AnularAsync(id, motivo, _userManager.GetUserId(User)!),
            "Compra Anulada : stock revertido");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _compraService.EliminarBorradorAsync(id);
            TempData["ok"] = "borrador eliminado";
            return RedirectToAction(nameof(Index));
        }
        catch (StockException ex)
        {
            TempData["error"] = ex.Message;
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }

    // es la busqueda de propductos y devuelve el costo directamente por eso esta aca y no en productos xd

    [HttpGet]
    public async Task<IActionResult> BuscarProducto(String? term)
    {
        var query = _context.Productos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(term)) query = query.Where(p => p.Nombre.Contains(term));

        var Items = await query
        .OrderBy(p => p.Nombre)
            .Take(10)
            .Select(p => new { id = p.Id, nombre = p.Nombre, stock = p.StockActual, costo = p.PrecioCosto })
            .ToListAsync();
        return Json(Items);
    }

    // larga una accion del service , que captura el error del negocio y vuelve al detalle 

    private async Task<IActionResult> Ejecutar(int id, Func<Task> accion, string mensajeOK)
    {
        try
        {
            await accion();
            TempData["ok"] = mensajeOK;
        }
        catch (StockException ex)
        {
            TempData["error"] = ex.Message;
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["error"] = "el stock cambio mientras operabas , intente nuevamente";
        }
        return RedirectToAction(nameof(Detalle), new { id });
    }

}