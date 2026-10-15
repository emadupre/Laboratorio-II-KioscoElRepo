using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Services;
using KioscoPOS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class StockController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stockService;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int  PageSize =10;

    public StockController(ApplicationDbContext context , IStockService stockService , UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _stockService = stockService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Historial(int? productoId, TipoMovimientoStock? tipo, DateTime? desde , DateTime? hasta, int page = 1)
    {
        page = Math.Max(page,1);
        var query = _context.MovimientosStock
        .Include(m => m.Producto)
        .Include(m => m.Usuario)
        .AsQueryable();

        if(productoId.HasValue) query = query.Where(m => m.ProductoId == productoId);
        if(tipo.HasValue) query = query.Where(m => m.Tipo == tipo);
        if(desde.HasValue)query = query.Where(m => m.Fecha >= desde.Value.Date);
        if(hasta.HasValue) query = query.Where(m => m.Fecha <= hasta.Value.AddDays(1).AddTicks(-1));

        var total = await query.CountAsync();

        var items = await query 
        .OrderByDescending(m => m.Fecha)
        .Skip((page - 1) * PageSize)
        .Take(PageSize)
        .ToListAsync();

        if(productoId.HasValue)
        ViewBag.ProductoNombre = await _context.Productos
        .Where(p => p.Id ==productoId)
        .Select(p => p.Nombre)
        .FirstOrDefaultAsync();

        ViewBag.ProductoId = productoId;
        ViewBag.Tipo = tipo;
        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        return View(new PagedResult<MovimientoStock>
        {
            Items = items, Page = page, PageSize = PageSize, TotalItems = total
        });
    }

    [Authorize(Roles = Roles.Administrador)]
    public IActionResult Ajuste() => View (new AjusteStockVM());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Ajuste(AjusteStockVM VM)
    {
        if(!ModelState.IsValid) return View(VM);

        var cantidad = VM.Operacion =="aumentar" ? VM.Cantidad : -VM.Cantidad;

        try
        {
            await _stockService.RegistrarMovimientoAsync(VM.ProductoId , cantidad ,TipoMovimientoStock.AjusteManual , _userManager.GetUserId(User)!, VM.Motivo);
            await _context.SaveChangesAsync();
        }
        catch (StockException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(VM);
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "El stock cambió mientras operabas. Volvé a intentar.");
            return View(VM);
        }
    TempData["ok"] = "Ajuste Registrado";
    return RedirectToAction(nameof(Historial),new {productoId = VM.ProductoId});
    }
}