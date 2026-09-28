using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioscoPOS.Web.Models;

public class Producto
{
  public int Id { get; set; }

  public int CategoriaId { get; set; } // el FK a Categoria
  public Categoria Categoria { get; set; } = null!;


  [Required(ErrorMessage = "El nombre es obligatorio")]
  [StringLength(120)]
  public string Nombre { get; set; } = null!;

  [StringLength(50)]
  public string? CodigoBarras { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  [Range(0, 9999999, ErrorMessage = "El precio debe ser mayor o igual a 0")]
  public decimal PrecioCosto { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  [Range(0, 9999999, ErrorMessage = "El precio debe ser mayor o igual a 0")]
  public decimal PrecioVenta { get; set; }

  [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
  public int StockActual { get; set; }

  [Range(0, int.MaxValue, ErrorMessage = "El stock minimo no puede ser negativo")]
  public int StockMinimo { get; set; }

  [StringLength(255)]
  public string? ImagenPath { get; set; }

  //activa el token de concurrencia. esto lo va a mapear a una columna rowversion/timestamp que mysql lo maneja solo, automaticamente.
  [Timestamp]
  public byte[]? RowVersion { get; set; }

  //naveg
  public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
  public ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();
  public ICollection<MovimientoStock> Movientos { get; set; } = new List<MovimientoStock>();

}