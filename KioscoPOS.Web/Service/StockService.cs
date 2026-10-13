using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;

namespace KioscoPOS.Web.Services;

public interface IStockService
{
    Task RegistrarMovimientoAsync(int productoId, int cantidad, TipoMovimientoStock tipo,
        string usuarioId, string? motivo = null, int? ventaId = null, int? compraId = null);
}

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context) => _context = context;

    // cantidad positiva suma, negativa resta. No hace SaveChanges: lo hace quien llama.
    public async Task RegistrarMovimientoAsync(int productoId, int cantidad, TipoMovimientoStock tipo,
        string usuarioId, string? motivo = null, int? ventaId = null, int? compraId = null)
    {
        if (cantidad == 0)
            throw new StockException("La cantidad no puede ser cero");

        var producto = await _context.Productos.FindAsync(productoId)
            ?? throw new StockException("El producto no existe");

        var nuevoStock = producto.StockActual + cantidad;
        if (nuevoStock < 0)
            throw new StockException($"Stock insuficiente de '{producto.Nombre}'. Disponible: {producto.StockActual}");

        producto.StockActual = nuevoStock;

        _context.MovimientosStock.Add(new MovimientoStock
        {
            ProductoId = productoId,
            Tipo = tipo,
            Cantidad = cantidad,
            StockResultante = nuevoStock,
            Motivo = motivo,
            VentaId = ventaId,
            CompraId = compraId,
            UsuarioId = usuarioId,
            Fecha = DateTime.Now
        });
    }
}