using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KioscoPOS.Web.Models.Enums;

namespace KioscoPOS.Web.Models;

public class Venta
{
  public int Id { get; set; }

  // el fk al usuario
  public string UsuarioId { get; set; } = null!;
  public ApplicationUser Usuario { get; set; } = null!;

  //fk a caja
  public int? CajaId { get; set; }
  public Caja? Caja { get; set; }
  //aca es nullable porque puede suceder de ventas sin caja

  public DateTime FechaHora { get; set; } = DateTime.Now;

  public MedioPago MedioPago { get; set; }

  public EstadoVenta Estado { get; set; } = EstadoVenta.Confirmada;

  [Column(TypeName = "decimal(18,2)")]
  public decimal Total { get; set; }

  //por si es aanulada
  public string? AnuladaPorId { get; set; }
  public DateTime? FechaAnulacion { get; set; }

  [StringLength(250)]
  public string? MotivoAnulacion { get; set; }

  //naveg.
  public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}