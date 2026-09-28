using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KioscoPOS.Web.Models.Enums;

namespace KioscoPOS.Web.Models; 

public class MovimientoCaja
{
  public int Id { get; set; }

  public int CajaId { get; set; }
  public Caja Caja { get; set; } = null!;

  public TipoMovimientoCaja Tipo { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal Monto { get; set; }

  [StringLength(250)]
  public string? Motivo { get; set; }

  //fk a usuario para auditoria
  public string UsuarioId { get; set; } = null!;
  public ApplicationUser Usuario { get; set; } = null!;

  public DateTime Fecha { get; set; } = DateTime.Now;
}