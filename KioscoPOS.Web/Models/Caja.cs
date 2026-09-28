using System.ComponentModel.DataAnnotations.Schema;
using KioscoPOS.Web.Models.Enums;

namespace KioscoPOS.Web.Models;

public class Caja
{
  public int Id { get; set; }

  // fk a usuario, como auditoria
  public string UsuarioId { get; set; } = null!;
  public ApplicationUser Usuario { get; set; } = null!;

  public Turno Turno { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal FondoInicial { get; set; }

  public DateTime FechaApertura { get; set; } = DateTime.Now;
  public DateTime? FechaCierre { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? TotalEsperado { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? TotalContado { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? Diferencia { get; set; }

  //naveg.
  public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
  public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
}