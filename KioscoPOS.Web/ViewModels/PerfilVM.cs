using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.ViewModels;

public class PerfilVM
{
  [Required(ErrorMessage = "El nombre es obligatorio")]
  [StringLength(100)]
  [Display(Name = "Nombre completo")]
  public string NombreCompleto { get; set; } = string.Empty;

  public string? AvatarPath { get; set; }

  [Display(Name = "Avatar")]
  public IFormFile? AvatarArchivo { get; set; }

  //solo para mostrar, de lectura
  public string? Email { get; set; }
  public string? Rol { get; set; }
}