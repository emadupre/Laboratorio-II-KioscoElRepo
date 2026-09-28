using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KioscoPOS.Web.Models.Enums;

namespace KioscoPOS.Web.Models;

public class MovimientoStock
{
  public int Id { get; set; }
  
  public int ProductoId { get; set; }
  public Producto Producto { get; set; } = null!;

  public TipoMovimientoStock Tipo { get; set; }

  // positivo entra en stock, negativo sale stock
  public int Cantidad { get; set; }

  // audotiria para el stock
  public int StockResultante { get; set; }

  [StringLength(250)]
  public string? Motivo { get; set; }
  
  // referencia opcional al origen del movimiento
  public int? VentaId { get; set; }
  public int? CompraId { get; set; }

  //fk a usuario para auditoria
  public string UsuarioId { get; set; } = null!;
  public ApplicationUser Usuario { get; set; } = null!;

  public DateTime Fecha { get; set; } = DateTime.Now;
}