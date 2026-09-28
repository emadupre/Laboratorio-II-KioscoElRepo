using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.Models;

public class ApplicationUser : IdentityUser
{
  [StringLength(100)]
  public string? NombreCompleto { get; set; }

  [StringLength(255)]
  public string? AvatarPath { get; set; }

  public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
  public ICollection<Compra> Compras { get; set; } = new List<Compra>();
  public ICollection<Caja> Cajas { get; set; } = new List<Caja>();
}