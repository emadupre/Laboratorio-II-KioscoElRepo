using System.Globalization;
using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Service;
using KioscoPOS.Web.Services;
using KioscoPOS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MovimientosCajaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICajaService _cajaService;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int PageSize = 10;

    public MovimientosCajaController(ApplicationDbContext context, ICajaService cajaService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _cajaService = cajaService;
        _userManager = userManager;
    }

    // sin cajaId muestra la caja abierta del usuario; con cajaId, esa caja (si le corresponde)
    public async Task<IActionResult> Index(int? cajaId, int page = 1)
    {
        var userId = _userManager.GetUserId(User)!;
        var esAdmin = User.IsInRole(Roles.Administrador);

        Caja? caja;
        if (cajaId == null)
            caja = await _cajaService.ObtenerCajaAbiertaAsync(userId);
        else
            caja = await _context.Cajas.Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == cajaId);

        if (caja == null)
        {
            TempData["Error"] = cajaId == null ? "No tenés una caja abierta" : "La caja no existe";
            return RedirectToAction("Index", "Caja");
        }

        if (!esAdmin && caja.UsuarioId != userId)
            return Forbid();

        var id = caja.Id;
        var query = _context.MovimientosCaja.AsNoTracking()
            .Include(m => m.Usuario)
            .Where(m => m.CajaId == id);

        page = Math.Max(page, 1);
        var total = await query.CountAsync();
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));

        var items = await query
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new MovimientosCajaVM
        {
            Caja = caja,
            PuedeRegistrar = caja.FechaCierre == null && caja.UsuarioId == userId,
            Movimientos = new PagedResult<MovimientoCaja>
            {
                Items = items, Page = page, PageSize = PageSize, TotalItems = total
            }
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int cajaId, TipoMovimientoCaja tipo, string monto, string? motivo)
    {
        if (!decimal.TryParse(monto, NumberStyles.Number, CultureInfo.InvariantCulture, out var importe))
        {
            TempData["Error"] = "El monto no es válido";
            return RedirectToAction(nameof(Index), new { cajaId });
        }

        try
        {
            await _cajaService.RegistrarMovimientoAsync(
                cajaId, _userManager.GetUserId(User)!, tipo, importe, motivo);
            TempData["Ok"] = tipo == TipoMovimientoCaja.Ingreso ? "Ingreso registrado" : "Retiro registrado";
        }
        catch (CajaException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { cajaId });
    }
}