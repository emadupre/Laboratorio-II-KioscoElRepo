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
public class CajasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICajaService _cajaService;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int PageSize = 10;

    public CajasController(ApplicationDbContext context, ICajaService cajaService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _cajaService = cajaService;
        _userManager = userManager;
    }

    // panel: si hay caja abierta muestra el resumen y el cierre, si no, el formulario de apertura
    public async Task<IActionResult> Index()
    {
        var caja = await _cajaService.ObtenerCajaAbiertaAsync(_userManager.GetUserId(User)!);

        var vm = new CajaPanelVM { Caja = caja };
        if (caja != null)
            vm.Resumen = await _cajaService.CalcularResumenAsync(caja.Id);

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(Turno turno, string fondoInicial)
    {
        if (!TryParseMonto(fondoInicial, out var fondo))
        {
            TempData["Error"] = "El fondo inicial no es válido";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _cajaService.AbrirAsync(_userManager.GetUserId(User)!, turno, fondo);
            TempData["Ok"] = "Caja abierta";
        }
        catch (CajaException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(int id, string totalContado)
    {
        if (!TryParseMonto(totalContado, out var contado))
        {
            TempData["Error"] = "Tenés que cargar el efectivo contado para cerrar la caja";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var caja = await _cajaService.CerrarAsync(id, _userManager.GetUserId(User)!, contado);
            TempData["Ok"] = $"Caja cerrada. Diferencia: $ {caja.Diferencia:N2}";
            return RedirectToAction(nameof(Historial));
        }
        catch (CajaException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // el admin ve todas las cajas, el cajero solo las suyas
    public async Task<IActionResult> Historial(int page = 1)
    {
        page = Math.Max(page, 1);

        var query = _context.Cajas.AsNoTracking().Include(c => c.Usuario).AsQueryable();

        if (!User.IsInRole(Roles.Administrador))
        {
            var userId = _userManager.GetUserId(User)!;
            query = query.Where(c => c.UsuarioId == userId);
        }

        var total = await query.CountAsync();
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));

        var items = await query
            .OrderByDescending(c => c.FechaApertura).ThenByDescending(c => c.Id)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(new PagedResult<Caja>
        {
            Items = items, Page = page, PageSize = PageSize, TotalItems = total
        });
    }

    // el input type=number siempre manda el punto como decimal, por eso InvariantCulture
    private static bool TryParseMonto(string? texto, out decimal monto) =>
        decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out monto);
}