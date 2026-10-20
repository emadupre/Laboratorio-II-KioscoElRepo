using KioscoPOS.Web.Models;

namespace KioscoPOS.Web.ViewModels;

public class MovimientosCajaVM
{
    public Caja Caja { get; set; } = null!;

    public PagedResult<MovimientoCaja> Movimientos { get; set; } = new();

    // true solo si la caja esta abierta y es del usuario logueado
    public bool PuedeRegistrar { get; set; }
}