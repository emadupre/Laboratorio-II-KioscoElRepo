using KioscoPOS.Web.Models;
using KioscoPOS.Web.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Data.Seed;

public static class DbSeeder
{
  public static async Task SeedAsync(IServiceProvider sp)
  {
    var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var db = sp.GetRequiredService<ApplicationDbContext>();

    //roles
    foreach (var rol in new[]
    {
      Roles.Administrador, Roles.Cajero
    })
    {
      if (!await roleMgr.RoleExistsAsync(rol))
      {
        await roleMgr.CreateAsync(new IdentityRole(rol));
      }
    }

    //usuarios demo
    await CrearUsuarioAsync(userMgr,
    email: "admin@kiosco.com",
    password: "Admin#2026",
    nombreCompleto: "Admin Demo",
    rol: Roles.Administrador);

    await CrearUsuarioAsync(userMgr,
    email: "cajero@kiosco.com",
    password: "Cajero#2026",
    nombreCompleto: "Cajero Demo",
    rol: Roles.Cajero);

    //cat. base
    if (!await db.Categorias.AnyAsync())
    {
      db.Categorias.AddRange(
        new Categoria { Nombre = "Bebidas" },
        new Categoria { Nombre = "Cigarrillos" },
        new Categoria { Nombre = "Golosinas" },
        new Categoria { Nombre = "Snacks" },
        new Categoria { Nombre = "Almacén" });

      await db.SaveChangesAsync();
    }
  }

  private static async Task CrearUsuarioAsync(
    UserManager<ApplicationUser> userMgr,
    string email,
    string password,
    string nombreCompleto,
    string rol)
  {
    var existente = await userMgr.FindByEmailAsync(email);
    if (existente is not null) return;

    var usuario = new ApplicationUser
    {
      UserName = email,
      Email = email,
      EmailConfirmed = true,
      NombreCompleto = nombreCompleto
    };

    var resultado = await userMgr.CreateAsync(usuario, password);

    if (!resultado.Succeeded)
    {
      var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
      throw new InvalidOperationException($"No se pudo crear {email}: {errores}");
    }

    await userMgr.AddToRoleAsync(usuario, rol);
  }
}