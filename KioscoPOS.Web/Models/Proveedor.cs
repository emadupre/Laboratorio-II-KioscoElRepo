using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.Models;

public class Proveedor
{
  public int Id { get; set; }
  [Required(ErrorMessage = "El nombre es obligatorio")]
  [StringLength(120, MinimumLength = 3, ErrorMessage ="El nombre debe tener entre 3 y 120 caracteres")]
  public string Nombre { get; set; } = null!;

  [StringLength(120)]
  [RegularExpression(@"^\d{2}-?\d{8}-?\d{1}$", ErrorMessage = "Formato esperado: XX-XXXXXXXX-X (solo numeros, guiones opcionales)")]
  public string? CUIT { get; set; }

  [StringLength(120)]
  [RegularExpression(@"^[\d\s\+\-\(\)]{6,20}$", ErrorMessage = "El contacto debe tener entre 6 y 20 caracteres y solo puede contener numeros y simbolos +, -,( ), espacios")]
  public string? Contacto { get; set; }

  //un proveedor tiene muchas compras
  public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}