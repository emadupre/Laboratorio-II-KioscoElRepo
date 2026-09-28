using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.Models;

public class Proveedor
{
  public int Id { get; set; }
  [Required(ErrorMessage = "El nombre es obligatorio")]
  [StringLength(120)]
  public string Nombre { get; set; } = null!;

  [StringLength(120)]
  public string? Contacto { get; set; }

  [StringLength(120)]
  public string? CUIT { get; set; }

  //un proveedor tiene muchas compras
  public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}