using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Services;

public interface IComprasService
{
    Task<int> CrearAsync(int proveedorId , DateTime fecha , string usuarioId);
    Task AgregarItemsAsync(int comprarId , int productoId , int cantidad , decimal costoUnitario);
    Task QuitarItemsAsync(int comprarId , int detalleID);
    Task EliminarBorradorAsync ( int comprarId);
    Task ConfirmarAsync ( int comprarId , string usuarioId);
    Task AnularAsync(int comprarId, string motivo , string usuarioId);

}

//los errores del negocio aparecen como stockSexception 
public class CompraService : IComprasService 
{
    private readonly ApplicationDbContext _context;
    private readonly IStockService _stock;

    public CompraService(ApplicationDbContext context , IStockService stock)
    {
        _context = context;
        _stock = stock;
    }
    private async Task<Compra> obtenerAsync(int id)=> await _context.Compras.Include(c => c.Detalles).FirstOrDefaultAsync(c=> c.Id == id)??
    throw new StockException("la compra no existe");

    private static void ExigirBorrador (Compra c)
    {
        if(c.Estado != EstadoCompra.Borrador)
        throw new StockException ("solo se puede modificar compras en borrador");
    }

    public async Task<int> CrearAsync(int proveedorId , DateTime fecha , string usuarioId)
    {
        if(!await _context.Proveedores.AnyAsync(p => p.Id == proveedorId))
        throw new StockException("el proveedor no existe");

        var compra = new Compra
        {
            ProveedorId = proveedorId,
            UsuarioId = usuarioId,
            Fecha = fecha,
            Estado = EstadoCompra.Borrador
        };

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();
        return compra.Id;
    }

    public async Task AgregarItemsAsync(int comprarId , int productoId , int cantidad , decimal costoUnitario)
    {
        var compra = await obtenerAsync(comprarId);
        ExigirBorrador(compra);

        if(cantidad <= 0) throw new StockException ("la cantidad debe ser mayor a 0 ");
        if(costoUnitario <= 0 ) throw new StockException("el costo debe ser mayor a 0");
        if(!await _context.Productos.AnyAsync(p => p.Id == productoId))
        throw new StockException("el producto no existe");

        // si el producto ya existe en la compra , se suma a la cantidad y queda el ultimo costo cargado en la compra
        var detalle = compra.Detalles.FirstOrDefault ( d => d.ProductoId == productoId);
        if(detalle == null)
        {
            compra.Detalles.Add(new DetalleCompra
            {
                ProductoId = productoId,
                Cantidad = cantidad,
                CostoUnitario = costoUnitario
            });
        }
        else
        {
            detalle.Cantidad += cantidad;
            detalle.CostoUnitario = costoUnitario;
        }
        compra.Total = compra.Detalles.Sum(d => d.Cantidad * d.CostoUnitario);
        await _context.SaveChangesAsync();
    }
    public async Task QuitarItemsAsync(int comprarId , int detalleID)
    {
        var compra = await obtenerAsync(comprarId);
        ExigirBorrador(compra);
        var detalle = compra.Detalles.FirstOrDefault(d => d.Id == detalleID)??
        throw new StockException ("el item no existe");
        _context.DetalleCompras.Remove(detalle);
        compra.Total = compra.Detalles.Where ( d => d.Id != detalleID).Sum(d => d.Cantidad * d.CostoUnitario);
        await _context.SaveChangesAsync();
    }

public async Task EliminarBorradorAsync(int comprarId)
    {
        var compra = await obtenerAsync(comprarId);
        ExigirBorrador(compra);
        _context.DetalleCompras.RemoveRange(compra.Detalles);
        _context.Compras.Remove(compra);
        await _context.SaveChangesAsync();
    }
    public async Task ConfirmarAsync(int comprarId , string usuarioId)
    {
        var compra = await obtenerAsync(comprarId);
        ExigirBorrador(compra);
        if(!compra.Detalles.Any())throw new StockException ("la compra no tiene items");

        await using var tx = await _context.Database.BeginTransactionAsync();

        foreach(var d in compra.Detalles)
        {
            await _stock.RegistrarMovimientoAsync(d.ProductoId , d.Cantidad,TipoMovimientoStock.Compra , usuarioId, $"compra #{compra.Id}", compraId : compra.Id);

        var producto = await _context.Productos.FindAsync(d.Producto);
        producto!.PrecioCosto = d.CostoUnitario; // este es el ultimo costo de compra
        }
        compra.Estado = EstadoCompra.Confirmada;
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task AnularAsync(int compraId , string motivo , string usuarioId)
    {
        
        var compra = await obtenerAsync(compraId);

        if(compra.Estado != EstadoCompra.Confirmada)
        throw new StockException ("solo se pueden anular compras confirmadas");
        if(string.IsNullOrWhiteSpace(motivo))throw new StockException("el motivo es obligatorio");
        await using var tx = await _context.Database.BeginTransactionAsync();

        //si el stock queda negativo , tira error y no se guarda nada 
        foreach (var d in compra.Detalles)
        {
            await _stock.RegistrarMovimientoAsync(d.ProductoId , -d.Cantidad,
            TipoMovimientoStock.AnulacionCompra , usuarioId , $"Anulacion compra #{compra.Id}",compraId : compra.Id);
        }
        compra.Estado = EstadoCompra.Anulada;
        compra.AnuladaPorId = usuarioId;
        compra.FechaAnulacion = DateTime.Now;
        compra.MotivoAnulacion = motivo.Trim();

        await _context.SaveChangesAsync();
        await tx.CommitAsync();
    }

}