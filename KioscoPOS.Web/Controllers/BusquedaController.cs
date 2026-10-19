using KioscoPOS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Controllers;

[Authorize]
public class BusquedaController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int MaxResultados = 10;
    private const int MinCaracteres = 2;

    public BusquedaController(ApplicationDbContext context)
    {
        _context = context;
    }

    // lo que devolvemos al navegador: NUNCA la entidad Producto completa
    public record ProductoBusquedaDto(
        int Id,
        string Nombre,
        string? CodigoBarras,
        decimal PrecioVenta,
        int StockActual,
        decimal? PrecioCosto);

    // autocomplete: /Busqueda/Productos?q=coca
    [HttpGet]
    public async Task<IActionResult> Productos(string? q)
    {
        q = q?.Trim();
        if (string.IsNullOrEmpty(q) || q.Length < MinCaracteres)
            return Json(Array.Empty<ProductoBusquedaDto>());

        var esAdmin = User.IsInRole("Administrador");

        var items = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Nombre.Contains(q) || (p.CodigoBarras != null && p.CodigoBarras.Contains(q)))
            .OrderBy(p => p.Nombre)
            .Take(MaxResultados)
            .Select(p => new ProductoBusquedaDto(
                p.Id,
                p.Nombre,
                p.CodigoBarras,
                p.PrecioVenta,
                p.StockActual,
                esAdmin ? p.PrecioCosto : (decimal?)null))
            .ToListAsync();

        return Json(items);
    }

    // lector de codigo de barras: /Busqueda/PorCodigo?codigo=7790895000010
    [HttpGet]
    public async Task<IActionResult> PorCodigo(string? codigo)
    {
        codigo = codigo?.Trim();
        if (string.IsNullOrEmpty(codigo))
            return BadRequest(new { mensaje = "El código es obligatorio" });

        var esAdmin = User.IsInRole("Administrador");

        var producto = await _context.Productos
            .AsNoTracking()
            .Where(p => p.CodigoBarras == codigo)
            .Select(p => new ProductoBusquedaDto(
                p.Id,
                p.Nombre,
                p.CodigoBarras,
                p.PrecioVenta,
                p.StockActual,
                esAdmin ? p.PrecioCosto : (decimal?)null))
            .FirstOrDefaultAsync();

        if (producto == null)
            return NotFound(new { mensaje = "No existe un producto con ese código" });

        return Json(producto);
    }
}