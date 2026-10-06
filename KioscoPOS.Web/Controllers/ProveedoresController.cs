using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ProveedoresController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public ProveedoresController (ApplicationDbContext context)
    {
        _context = context;
    }
// listado de busqueda para el paginado
public async Task<IActionResult>Index(string? buscar , int page = 1)
    {
    page = Math.Max(page, 1);
    var query = _context.Proveedores.AsQueryable();

    if(!string.IsNullOrWhiteSpace(buscar))
    {
        var termino = buscar.Trim();
        query = query.Where(p => p.Nombre.Contains(termino) || (p.CUIT != null && p.CUIT.Contains(termino)));
    }
    var total = await query.CountAsync();
    page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
    var items = await query
        .OrderBy(p=>p.Nombre)
        .Skip((page - 1)* PageSize)
        .Take(PageSize)
        .ToListAsync();

        ViewBag.Buscar = buscar;
        return View(new PagedResult<Proveedor>{
            Items = items,
            Page = page,
            PageSize = PageSize,
            TotalItems = total
        });
    }

//alta
public IActionResult Create() =>View();

[HttpPost , ValidateAntiForgeryToken]
public async Task<IActionResult>Create(Proveedor proveedor)
    {
        proveedor.CUIT = NormalizarCuit(proveedor.CUIT);
        if (proveedor.CUIT != null &&
            await _context.Proveedores.AnyAsync(p => p.CUIT == proveedor.CUIT))
            ModelState.AddModelError(nameof(Proveedor.CUIT), "Ya existe un proveedor con este CUIT.");

        if(!ModelState.IsValid)
        return View(proveedor);
        _context.Add(new Proveedor
        {
            Nombre = proveedor.Nombre,
            Contacto = proveedor.Contacto,
            CUIT = proveedor.CUIT
        });
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Proveedor creado";
        return RedirectToAction(nameof(Index));
    }

    //modificacion

    public async Task<IActionResult> Edit(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if(proveedor == null) return NotFound();
        return View(proveedor);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult>Edit(int id , Proveedor proveedor)
    {
        if(id !=proveedor.Id) return BadRequest();
        proveedor.CUIT = NormalizarCuit(proveedor.CUIT);
        if (proveedor.CUIT != null &&
            await _context.Proveedores.AnyAsync(p => p.CUIT == proveedor.CUIT && p.Id != id))
            ModelState.AddModelError(nameof(Proveedor.CUIT), "Ya existe un proveedor con este CUIT.");

        if(!ModelState.IsValid)
        return View(proveedor);

        var proveedorExistente = await _context.Proveedores.FindAsync(id);
        if (proveedorExistente == null) return NotFound();
        proveedorExistente.Nombre = proveedor.Nombre;
        proveedorExistente.Contacto = proveedor.Contacto;
        proveedorExistente.CUIT = proveedor.CUIT;
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Proveedor actualizado";
        return RedirectToAction(nameof(Index));
    }

    //baja solo para los admins

    [HttpPost , ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult>Delete (int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if(proveedor == null) return NotFound();

        if(await _context.Compras.AnyAsync(c => c.ProveedorId == id)){
            TempData["Error"] = "No se puede eliminar el proveedor: tiene compras registradas.";
            return RedirectToAction(nameof(Index));
        }
        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();
        TempData["Ok"]= "Proveedor eliminado";
        return RedirectToAction(nameof(Index));
    }

    private static string? NormalizarCuit(string? cuit)
    {
        if (string.IsNullOrWhiteSpace(cuit))
            return null;

        var normalizado = new string(cuit.Where(char.IsDigit).ToArray());
        return normalizado.Length == 0 ? null : normalizado;
    }
}