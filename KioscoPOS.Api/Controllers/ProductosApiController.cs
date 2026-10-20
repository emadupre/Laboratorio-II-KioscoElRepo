using KioscoPOS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize] // se protege con JWT
public class ProductosApiController : ControllerBase
{
  private readonly ApplicationDbContext _context;

  public ProductosApiController(ApplicationDbContext context) => _context = context;

  [HttpGet]
  public async Task<IActionResult> Listar(string? buscar, int page = 1, int pageSize = 10)
  {
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var query = _context.Productos.AsNoTracking().Include(p => p.Categoria).Include(p => p.Marca).AsQueryable();

    if (!string.IsNullOrWhiteSpace(buscar))
      query = query.Where(p => p.Nombre.Contains(buscar) || (p.CodigoBarras != null && p.CodigoBarras.Contains(buscar)));

    var total = await query.CountAsync();
    var items = await query
      .OrderBy(p => p.Nombre)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .Select(p => new
      {
        p.Id,
        p.Nombre,
        p.CodigoBarras,
        Categoria = p.Categoria.Nombre,
        Marca = p.Marca.Nombre,
        p.PrecioVenta,
        p.StockActual,
        p.StockMinimo
      })
      .ToListAsync();

    return Ok(new
    {
      items,
      page,
      pageSize,
      totalItems = total,
      totalPages = (int)Math.Ceiling(total / (double)pageSize)
    });
  }
  [HttpGet("{id:int}")]
  public async Task<IActionResult> Detalle(int id)
  {
    var producto = await _context.Productos.AsNoTracking()
    .Include(p => p.Categoria)
    .Include(p => p.Marca)
    .Where(p => p.Id == id)
    .Select(p => new
    {
      p.Id,
      p.Nombre,
      p.CodigoBarras,
      Categoria = p.Categoria.Nombre,
      Marca = p.Marca.Nombre,
      p.PrecioVenta,
      p.StockActual,
      p.StockMinimo,
      p.ImagenPath
    })
    .FirstOrDefaultAsync();

    if (producto == null) return NotFound(new { mensaje = "Producto no encontrado" });
    return Ok(producto);
  }
}