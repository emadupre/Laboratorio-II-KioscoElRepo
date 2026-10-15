using System.Xml.Schema;
using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Controllers;

[Authorize]
public class MarcasController : Controller
{
  private readonly ApplicationDbContext _context;

  public MarcasController(ApplicationDbContext context)
  {
    _context = context;
  }

  //vista vue
  public IActionResult Gestion() => View();

  //endpoint json

  [HttpGet]
  public async Task<IActionResult> ListarJson(string? buscar, int page = 1, int pageSize = 10)
  {
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 50);

    var query = _context.Marcas.AsNoTracking().AsQueryable();
    if (!string.IsNullOrWhiteSpace(buscar))
      query = query.Where(m => m.Nombre.Contains(buscar));

    var total = await query.CountAsync();
    var items = await query
    .OrderBy(m => m.Nombre)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(m => new { m.Id, m.Nombre })
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

  public record MarcaDto(int Id, string? Nombre);

  [HttpPost]
  public async Task<IActionResult> CrearJson([FromBody] MarcaDto dto)
  {
    var nombre = dto?.Nombre?.Trim();
    if (string.IsNullOrWhiteSpace(nombre))
      return BadRequest(new { mensaje = "El nombre es obligatorio" });
    if (await _context.Marcas.AnyAsync(m => m.Nombre == nombre))
      return Conflict(new { mensaje = "Ya existe una marca con ese nombre" });

    var marca = new Marca { Nombre = nombre };
    _context.Add(marca);
    await _context.SaveChangesAsync();
    return Ok(new { marca.Id, marca.Nombre });
  }

  [HttpPut]
  public async Task<IActionResult> EditarJson(int id, [FromBody] MarcaDto dto)
  {
    if (dto == null || id != dto.Id)
      return BadRequest(new { mensaje = "Id no coincide" });

    var nombre = dto.Nombre?.Trim();
    if (string.IsNullOrWhiteSpace(nombre))
      return BadRequest(new { mensaje = "El nombre es obligatorio" });

    var marca = await _context.Marcas.FindAsync(id);
    if (marca == null) return NotFound(new { mensaje = "Marca no encontrada" });

    if (await _context.Marcas.AnyAsync(m => m.Nombre == nombre && m.Id != id))
      return Conflict(new { mensaje = "Ya existe una marca con ese nombre" });

    marca.Nombre = nombre;
    await _context.SaveChangesAsync();
    return Ok(new { marca.Id, marca.Nombre });
  }

  [HttpDelete]
  [Authorize(Roles = "Administrador")]
  public async Task<IActionResult> EliminarJson(int id)
  {
    var marca = await _context.Marcas.FindAsync(id);
    if (marca == null) return NotFound(new { mensaje = "Marca no encontrada" });

    if (await _context.Productos.AnyAsync(p => p.MarcaId == id))
      return Conflict(new { mensaje = "No se puede eliminar: tiene productos asociados" });

    _context.Marcas.Remove(marca);
    await _context.SaveChangesAsync();
    return Ok(new { mensaje = "Marca eliminada" });
  }

  //autocompletar para el form de productos
  [HttpGet]
  public async Task<IActionResult> Buscar(string? term)
  {
    var query = _context.Marcas.AsQueryable();
    if (!string.IsNullOrWhiteSpace(term))
      query = query.Where(m => m.Nombre.Contains(term));

    var items = await query
      .OrderBy(m => m.Nombre)
      .Take(10)
      .Select(m => new { id = m.Id, nombre = m.Nombre })
      .ToListAsync();

    return Json(items);
  }
}

