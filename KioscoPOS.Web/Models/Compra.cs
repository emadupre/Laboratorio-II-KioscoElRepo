using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KioscoPOS.Web.Models.Enums;

namespace KioscoPOS.Web.Models;

public class Compra
{
  public int Id { get; set; }

  //fk al Proveedor
  public int ProveedorId { get; set; }
  public Proveedor Proveedor { get; set; } = null!;

  //fk a usuario (acá lo podemos ver como una especie de auditoria)
  public string UsuarioId { get; set; } = null!;
  public ApplicationUser Usuario { get; set; } = null!;

  public DateTime Fecha { get; set; } = DateTime.Now;

  public EstadoCompra Estado { get; set; } = EstadoCompra.Borrador;

  [Column(TypeName = "decimal(18,2)")]
  public decimal Total { get; set; }

  // naveg.
  public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}