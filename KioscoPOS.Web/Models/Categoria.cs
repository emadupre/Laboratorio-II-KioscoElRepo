using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.Models;

public class Categoria
{
  public int Id { get; set; }

  [Required(ErrorMessage = "El nombre es obligatorio")]
  [StringLength(80, ErrorMessage = "Máximo 80 caracteres")]
  public string Nombre { get; set; } = null!;

  public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}