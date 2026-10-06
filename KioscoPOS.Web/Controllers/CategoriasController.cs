using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Authorize]
public class CategoriasController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;
    public CategoriasController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index(string? buscar, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _context.Categorias.AsQueryable();
    if (!string.IsNullOrWhiteSpace(buscar))
    query = query.Where(c => c.Nombre.Contains(buscar));

    var total = await query.CountAsync();
    page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));

    var items = await query
    .OrderBy(c => c.Nombre)
    .Skip((page-1)* PageSize)
    .Take(PageSize)
    .ToListAsync();

    ViewBag.Buscar = buscar;
    return View(new PagedResult<Categoria>
    {
        Items = items,
        Page = page,
        PageSize = PageSize,
        TotalItems = total
    });
    }

// alta

public IActionResult Create() => View(new Categoria());

[HttpPost , ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Categoria categoria)
    {
        if(await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
        ModelState.AddModelError("Nombre", "Ya Existe una categoria con este nombre ");

        if(!ModelState.IsValid)
        return View(categoria);

        _context.Add(new Categoria { Nombre = categoria.Nombre });
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Categoría creada";
        return RedirectToAction(nameof(Index));
    }

// modificar
public async Task<IActionResult> Edit(int Id)
    {
        var categoria = await _context.Categorias.FindAsync(Id);
        if (categoria == null) return NotFound();
        return View(categoria);
    }

    [HttpPost , ValidateAntiForgeryToken]
    public async Task<IActionResult>Edit(int id , Categoria categoria)
    {
        if(id !=categoria.Id) return BadRequest();

        if(await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre && c.Id != id))
            ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre.");

        if(!ModelState.IsValid)
        return View(categoria);

        var categoriaExistente = await _context.Categorias.FindAsync(id);
        if (categoriaExistente == null) return NotFound();
        categoriaExistente.Nombre = categoria.Nombre;
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Categoría actualizada";
        return RedirectToAction(nameof(Index));
    }

//baja, solo para los administradores
[HttpPost,ValidateAntiForgeryToken]
[Authorize(Roles = "Administrador")]
public async Task<IActionResult>Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if(categoria == null) return NotFound();

        if ( await _context.Productos.AnyAsync(p => p.CategoriaId == id))
        {
            TempData["Error"] = "No se puede eliminar la categoría: tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Categoría eliminada";
        return RedirectToAction(nameof(Index));
    }


    //busqueda de categoria

    [HttpGet]
public async Task<IActionResult> Buscar(string? term)
{
    var query = _context.Categorias.AsQueryable();

    if (!string.IsNullOrWhiteSpace(term))
        query = query.Where(c => c.Nombre.Contains(term));

    var items = await query
        .OrderBy(c => c.Nombre)
        .Take(10)
        .Select(c => new { id = c.Id, nombre = c.Nombre })
        .ToListAsync();

    return Json(items);
}
}