using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.ViewModels;

public class IniciarSesionVM
{
  [Required(ErrorMessage = "El email es obligatorio")]
  [EmailAddress(ErrorMessage = "Email invalido")]
  [Display(Name = "Email")]
  public string Email { get; set; } = null!;

  [Required(ErrorMessage = "La contraseña es obligatoria")]
  [DataType(DataType.Password)]
  [Display(Name = "Contraseña")]
  public string Password { get; set; } = null!;

  [Display(Name = "Recordarme")]
  public bool RememberMe{ get; set; }
}