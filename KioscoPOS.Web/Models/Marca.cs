using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.Models
{
  public class Marca
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
    public string Nombre { get; set; } = null!;

    //prop de naveg: Una Marca tiene muchos Productos (1:N)
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
  }
}