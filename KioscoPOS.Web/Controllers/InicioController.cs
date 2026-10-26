using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Services;
using KioscoPOS.Web.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KioscoPOS.Web.ViewModels;

namespace KioscoPOS.Web.Controllers;

public class InicioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICajaService _cajaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public InicioController(ApplicationDbContext context, ICajaService cajaService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _cajaService = cajaService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return View(new InicioVM());

        var userId = _userManager.GetUserId(User)!;
        var hoy = DateTime.Today;

        var vm = new InicioVM
        {
            TotalProductos = await _context.Productos.CountAsync(),
            ProductosStockBajo = await _context.Productos.CountAsync(p => p.StockActual <= p.StockMinimo),
            VentasHoy = await _context.Ventas.CountAsync(v => v.FechaHora.Date == hoy && v.Estado == EstadoVenta.Confirmada),
            TotalVentasHoy = await _context.Ventas.Where(v => v.FechaHora.Date == hoy && v.Estado == EstadoVenta.Confirmada).SumAsync(v => (decimal?)v.Total) ?? 0m,
            TieneCajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync(userId) != null,
            EsAdmin = User.IsInRole(Roles.Administrador)
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
