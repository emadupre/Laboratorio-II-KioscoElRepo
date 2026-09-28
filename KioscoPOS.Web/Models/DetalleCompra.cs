using System.ComponentModel.DataAnnotations.Schema;

namespace KioscoPOS.Web.Models;

public class DetalleCompra
{
  public int Id { get; set; }

  public int CompraId { get; set; }
  public Compra Compra { get; set; } = null!;

  public int ProductoId { get; set; }
  public Producto Producto { get; set; } = null!;

  public int Cantidad { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal CostoUnitario { get; set; }
}