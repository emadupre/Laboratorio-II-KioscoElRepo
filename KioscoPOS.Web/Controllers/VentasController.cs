using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Service;
using KioscoPOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class VentasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVentaService _ventaService;
    private readonly ICajaService _cajaService;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int PageSize = 10;

    public VentasController(ApplicationDbContext context, IVentaService ventaService,
        ICajaService cajaService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _ventaService = ventaService;
        _cajaService = cajaService;
        _userManager = userManager;
    }

    // lo que manda el carrito al confirmar (sin precios)
    public record ItemVentaDto(int ProductoId, int Cantidad);
    public record ConfirmarVentaDto(MedioPago MedioPago, List<ItemVentaDto>? Items);

    // el admin ve todas las ventas, el cajero solo las suyas
    public async Task<IActionResult> Index(EstadoVenta? estado, int page = 1)
    {
        var query = _context.Ventas.AsNoTracking().Include(v => v.Usuario).AsQueryable();

        if (!User.IsInRole(Roles.Administrador))
        {
            var userId = _userManager.GetUserId(User)!;
            query = query.Where(v => v.UsuarioId == userId);
        }

        if (estado != null)
            query = query.Where(v => v.Estado == estado);

        page = Math.Max(page, 1);
        var total = await query.CountAsync();
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));

        var items = await query
            .OrderByDescending(v => v.FechaHora).ThenByDescending(v => v.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Estado = estado;

        return View(new PagedResult<Venta>
        {
            Items = items, Page = page, PageSize = PageSize, TotalItems = total
        });
    }

    // pantalla de venta: sin caja abierta no tiene sentido armar un carrito
    public async Task<IActionResult> Nueva()
    {
        var caja = await _cajaService.ObtenerCajaAbiertaAsync(_userManager.GetUserId(User)!);
        if (caja == null)
        {
            TempData["Error"] = "Tenés que abrir la caja antes de vender";
            return RedirectToAction("Index", "Cajas");
        }

        return View();
    }

    // la llama el carrito con fetch y recibe JSON
    [HttpPost]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarVentaDto dto)
    {
        if (dto?.Items == null || dto.Items.Count == 0)
            return BadRequest(new { mensaje = "La venta no tiene productos" });

        try
        {
            var items = dto.Items.Select(i => new ItemVenta(i.ProductoId, i.Cantidad));
            var id = await _ventaService.ConfirmarAsync(_userManager.GetUserId(User)!, dto.MedioPago, items);
            return Ok(new { id, mensaje = "Venta registrada" });
        }
        catch (Exception ex) when (ex is VentaException or StockException)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { mensaje = "El stock cambió mientras vendías. Revisá el carrito e intentá de nuevo" });
        }
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _context.Ventas.AsNoTracking()
            .Include(v => v.Usuario)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        if (!User.IsInRole(Roles.Administrador) && venta.UsuarioId != _userManager.GetUserId(User))
            return Forbid();

        if (venta.AnuladaPorId != null)
            ViewBag.AnuladaPor = (await _userManager.FindByIdAsync(venta.AnuladaPorId))?.Email;

        return View(venta);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Anular(int id, string motivo)
    {
        try
        {
            await _ventaService.AnularAsync(id, motivo, _userManager.GetUserId(User)!);
            TempData["Ok"] = "Venta anulada. El stock fue devuelto";
        }
        catch (Exception ex) when (ex is VentaException or StockException)
        {
            TempData["Error"] = ex.Message;
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "El stock cambió mientras se anulaba. Intentá de nuevo";
        }

        return RedirectToAction(nameof(Detalle), new { id });
    }
}