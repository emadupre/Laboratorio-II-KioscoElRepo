using KioscoPOS.Web.Data;
using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using KioscoPOS.Web.Service;
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
}

public class CajaService : ICajaService
{
    private readonly ApplicationDbContext _context;

    public CajaService(ApplicationDbContext context) => _context = context;

    // una caja esta abierta mientras no tenga fecha de cierre
    public async Task<Caja?> ObtenerCajaAbiertaAsync(string usuarioId)
    {
        return await _context.Cajas
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
}