using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Services;

// los numeros de una caja en un momento dado
public record ResumenCaja(
    decimal FondoInicial,
    decimal VentasEfectivo,
    decimal Ingresos,
    decimal Retiros,
    decimal TotalEsperado);

public interface ICajaService
{
    Task<Caja?> ObtenerCajaAbiertaAsync(string usuarioId);
    Task<Caja> AbrirAsync(string usuarioId, Turno turno, decimal fondoInicial);
    Task<ResumenCaja> CalcularResumenAsync(int cajaId);
    Task<Caja> CerrarAsync(int cajaId, string usuarioId, decimal totalContado);

    Task<MovimientoCaja> RegistrarMovimientoAsync(int cajaId, string usuarioId, TipoMovimientoCaja tipo, decimal monto, string? motivo);
}

public class CajaService : ICajaService
{
    private readonly ApplicationDbContext _context;

    public CajaService(ApplicationDbContext context) => _context = context;

    // una caja esta abierta mientras no tenga fecha de cierre
    public async Task<Caja?> ObtenerCajaAbiertaAsync(string usuarioId)
    {
        return await _context.Cajas
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.FechaCierre == null);
    }

    public async Task<Caja> AbrirAsync(string usuarioId, Turno turno, decimal fondoInicial)
    {
        if (!Enum.IsDefined(turno))
            throw new CajaException("El turno no es válido");

        if (fondoInicial < 0)
            throw new CajaException("El fondo inicial no puede ser negativo");

        if (await ObtenerCajaAbiertaAsync(usuarioId) != null)
            throw new CajaException("Ya tenés una caja abierta. Cerrala antes de abrir otra");

        var caja = new Caja
        {
            UsuarioId = usuarioId,
            Turno = turno,
            FondoInicial = fondoInicial,
            FechaApertura = DateTime.Now
        };

        _context.Cajas.Add(caja);
        await _context.SaveChangesAsync();
        return caja;
    }

    // esperado = fondo + ventas en efectivo + ingresos - retiros
    public async Task<ResumenCaja> CalcularResumenAsync(int cajaId)
    {
        var caja = await _context.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cajaId)
            ?? throw new CajaException("La caja no existe");

        var ventasEfectivo = await _context.Ventas
            .Where(v => v.CajaId == cajaId
                    && v.MedioPago == MedioPago.Efectivo
                    && v.Estado == EstadoVenta.Confirmada)
            .SumAsync(v => (decimal?)v.Total) ?? 0m;

        var ingresos = await _context.MovimientosCaja
            .Where(m => m.CajaId == cajaId && m.Tipo == TipoMovimientoCaja.Ingreso)
            .SumAsync(m => (decimal?)m.Monto) ?? 0m;

        var retiros = await _context.MovimientosCaja
            .Where(m => m.CajaId == cajaId && m.Tipo == TipoMovimientoCaja.Retiro)
            .SumAsync(m => (decimal?)m.Monto) ?? 0m;

        var esperado = caja.FondoInicial + ventasEfectivo + ingresos - retiros;

        return new ResumenCaja(caja.FondoInicial, ventasEfectivo, ingresos, retiros, esperado);
    }

    public async Task<Caja> CerrarAsync(int cajaId, string usuarioId, decimal totalContado)
    {
        if (totalContado < 0)
            throw new CajaException("El efectivo contado no puede ser negativo");

        var caja = await _context.Cajas.FindAsync(cajaId)
            ?? throw new CajaException("La caja no existe");

        if (caja.FechaCierre != null)
            throw new CajaException("La caja ya está cerrada");

        if (caja.UsuarioId != usuarioId)
            throw new CajaException("Solo quien abrió la caja puede cerrarla");

        var resumen = await CalcularResumenAsync(cajaId);

        caja.TotalEsperado = resumen.TotalEsperado;
        caja.TotalContado = totalContado;
        caja.Diferencia = totalContado - resumen.TotalEsperado;
        caja.FechaCierre = DateTime.Now;

        await _context.SaveChangesAsync();
        return caja;
    }

//ingreso o retiro de efectivo durante el turno (no es una venta)
    public async Task<MovimientoCaja> RegistrarMovimientoAsync(int cajaId, string usuarioId, TipoMovimientoCaja tipo, decimal monto, string? motivo)
    {
        if (!Enum.IsDefined(tipo))
            throw new CajaException("El tipo de movimiento no es válido");

        if (monto <= 0)
            throw new CajaException("El monto debe ser mayor a cero");

        motivo = motivo?.Trim();
        if (string.IsNullOrEmpty(motivo))
            throw new CajaException("El motivo es obligatorio");

        if (motivo.Length > 250)
            throw new CajaException("El motivo no puede superar los 250 caracteres");

        var caja = await _context.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cajaId)
            ?? throw new CajaException("La caja no existe");

        if (caja.FechaCierre != null)
            throw new CajaException("La caja está cerrada");

        if (caja.UsuarioId != usuarioId)
            throw new CajaException("Solo quien abrió la caja puede registrar movimientos");

        // no se puede sacar mas efectivo del que hay en el cajon
        if (tipo == TipoMovimientoCaja.Retiro)
        {
            var resumen = await CalcularResumenAsync(cajaId);
            if (monto > resumen.TotalEsperado)
                throw new CajaException(
                    $"No hay suficiente efectivo en caja. Disponible: $ {resumen.TotalEsperado:N2}");
        }

        var movimiento = new MovimientoCaja
        {
            CajaId = cajaId,
            Tipo = tipo,
            Monto = monto,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Fecha = DateTime.Now
        };

        _context.MovimientosCaja.Add(movimiento);
        await _context.SaveChangesAsync();
        return movimiento;

    }
}