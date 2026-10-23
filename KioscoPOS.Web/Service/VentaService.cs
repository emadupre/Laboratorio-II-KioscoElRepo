using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Service;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Services;

// lo unico que manda el carrito: que producto y cuantos (el precio lo pone el servidor)
public record ItemVenta(int ProductoId, int Cantidad);

public interface IVentaService
{
    Task<int> ConfirmarAsync(string usuarioId, MedioPago medioPago, IEnumerable<ItemVenta> items);
    Task AnularAsync(int ventaId, string motivo, string usuarioId);
}

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stock;
    private readonly ICajaService _cajaService;

    public VentaService(ApplicationDbContext context, IStockService stock, ICajaService cajaService)
    {
        _context = context;
        _stock = stock;
        _cajaService = cajaService;
    }

    public async Task<int> ConfirmarAsync(string usuarioId, MedioPago medioPago, IEnumerable<ItemVenta> items)
    {
        var lista = items?.ToList() ?? new List<ItemVenta>();

        if (lista.Count == 0)
            throw new VentaException("La venta no tiene productos");

        if (!Enum.IsDefined(medioPago))
            throw new VentaException("El medio de pago no es válido");

        // se valida cada item ANTES de agrupar: si no, un -2 y un +2 se cancelarian entre si
        if (lista.Any(i => i.Cantidad <= 0))
            throw new VentaException("Todas las cantidades deben ser mayores a cero");

        var caja = await _cajaService.ObtenerCajaAbiertaAsync(usuarioId)
            ?? throw new VentaException("Tenés que abrir la caja antes de vender");

        // si el mismo producto viene dos veces, se suman las cantidades
        var agrupados = lista
            .GroupBy(i => i.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        var ids = agrupados.Select(a => a.ProductoId).ToList();
        var productos = await _context.Productos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (productos.Count != ids.Count)
            throw new VentaException("Alguno de los productos no existe");

        var venta = new Venta
        {
            UsuarioId = usuarioId,
            CajaId = caja.Id,
            FechaHora = DateTime.Now,
            MedioPago = medioPago,
            Estado = EstadoVenta.Confirmada
        };

        foreach (var a in agrupados)
        {
            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = a.ProductoId,
                Cantidad = a.Cantidad,
                // el precio sale de la base, nunca del navegador
                PrecioUnitario = productos[a.ProductoId].PrecioVenta
            });
        }

        venta.Total = venta.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        // si algo falla (por ejemplo stock insuficiente) se deshace todo, incluida la venta
        await using var tx = await _context.Database.BeginTransactionAsync();

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync(); // necesitamos el Id de la venta para el movimiento de stock

        foreach (var d in venta.Detalles)
        {
            await _stock.RegistrarMovimientoAsync(d.ProductoId, -d.Cantidad,
                TipoMovimientoStock.Venta, usuarioId, $"Venta #{venta.Id}", ventaId: venta.Id);
        }

        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return venta.Id;
    }

    public async Task AnularAsync(int ventaId, string motivo, string usuarioId)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == ventaId)
            ?? throw new VentaException("La venta no existe");

        if (venta.Estado != EstadoVenta.Confirmada)
            throw new VentaException("La venta ya está anulada");

        motivo = motivo?.Trim() ?? "";
        if (motivo.Length == 0)
            throw new VentaException("El motivo es obligatorio");
        if (motivo.Length > 250)
            throw new VentaException("El motivo no puede superar los 250 caracteres");

        // si la caja ya se cerro, su total esperado quedo fijo y no se puede tocar
        if (venta.CajaId != null)
        {
            var caja = await _context.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == venta.CajaId);
            if (caja != null && caja.FechaCierre != null)
                throw new VentaException("No se puede anular: la caja de esta venta ya fue cerrada");
        }

        await using var tx = await _context.Database.BeginTransactionAsync();

        foreach (var d in venta.Detalles)
        {
            await _stock.RegistrarMovimientoAsync(d.ProductoId, d.Cantidad,
                TipoMovimientoStock.AnulacionVenta, usuarioId, $"Anulación venta #{venta.Id}", ventaId: venta.Id);
        }

        venta.Estado = EstadoVenta.Anulada;
        venta.AnuladaPorId = usuarioId;
        venta.FechaAnulacion = DateTime.Now;
        venta.MotivoAnulacion = motivo;

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }
}