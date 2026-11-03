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

    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> ListarJson(string? buscar, int page = 1, int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.Categorias.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(c => c.Nombre.Contains(buscar));

        var total = await query.CountAsync();
        var items = await query
        .OrderBy(c => c.Nombre)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new { c.Id, c.Nombre })
        .ToListAsync();

        return Json(new
        {
            items,
            page,
            pageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    public record CategoriaDto(int Id, string? Nombre);

    [HttpPost]
    public async Task<IActionResult> CrearJson([FromBody] CategoriaDto dto)
    {
        var nombre = dto?.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio" });

        if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre))
            return Conflict(new { mensaje = "Ya existe una categoria con ese nombre." });

        var cat = new Categoria { Nombre = nombre };
        _context.Add(cat);
        await _context.SaveChangesAsync();
        return Ok(new { cat.Id, cat.Nombre });
    }

    [HttpPut]
    public async Task<IActionResult> EditarJson(int id, [FromBody] CategoriaDto dto)
    {
        if (dto == null || id != dto.Id)
            return BadRequest(new { mensaje = "Id no coincide" });

        var nombre = dto.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio" });

        var cat = await _context.Categorias.FindAsync(id);
        if (cat == null) return NotFound(new { mensaje = "Categoria no" });

        if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre && c.Id != id))
            return Conflict(new { mensaje = "Ya existe una categoria con ese nombre" });

        cat.Nombre = nombre;
        await _context.SaveChangesAsync();
        return Ok(new { cat.Id, cat.Nombre });
    }

    [HttpDelete]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> EliminarJson(int id)
    {
        var cat = await _context.Categorias.FindAsync(id);
        if (cat == null) return NotFound(new { mensaje = "Categoría no encontrada" });

        if (await _context.Productos.AnyAsync(p => p.CategoriaId == id))
            return Conflict(new { mensaje = "No se puede eliminar: tiene productos asociados." });

        _context.Categorias.Remove(cat);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Categoría eliminada" });
    }
}