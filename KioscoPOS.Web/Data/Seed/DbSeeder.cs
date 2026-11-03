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

    //marcas base
    if (!await db.Marcas.AnyAsync())
    {
      db.Marcas.AddRange(
      new() { Nombre = "Coca-Cola" },
      new() { Nombre = "Pepsi" },
      new() { Nombre = "Marlboro" },
      new() { Nombre = "Philip Morris" },
      new() { Nombre = "Arcor" },
      new() { Nombre = "Bagley" },
      new() { Nombre = "Marolio" },
      new() { Nombre = "Matarazzo" },
      new() { Nombre = "La Serenísima" },
      new() { Nombre = "Sin marca" }
      );
      await db.SaveChangesAsync();
    }
    // productos pre cargados 

    var categoriasDemo = new List<string> { "Bebidas", "Golosinas", "Snacks" };
    var categoriasExistentes = await db.Categorias
      .Where(c => categoriasDemo.Contains(c.Nombre))
      .Select(c => c.Nombre)
      .ToListAsync();
    foreach (var nombre in categoriasDemo.Except(categoriasExistentes))
    {
      db.Categorias.Add(new Categoria { Nombre = nombre });
    }

    var marcasDemo = new List<string> { "Coca-Cola", "Arcor", "Pepsi" };
    var marcasExistentes = await db.Marcas
      .Where(m => marcasDemo.Contains(m.Nombre))
      .Select(m => m.Nombre)
      .ToListAsync();
    foreach (var nombre in marcasDemo.Except(marcasExistentes))
    {
      db.Marcas.Add(new Marca { Nombre = nombre });
    }

    var proveedoresDemo = new List<string>
    {
      "Distribuidora Central",
      "Bebidas del Sur",
      "Mayorista Don Juan"
    };
    var proveedoresExistentes = await db.Proveedores
      .Where(p => proveedoresDemo.Contains(p.Nombre))
      .Select(p => p.Nombre)
      .ToListAsync();
    foreach (var nombre in proveedoresDemo.Except(proveedoresExistentes))
    {
      db.Proveedores.Add(new Proveedor { Nombre = nombre });
    }

    await db.SaveChangesAsync();

    var categoriaPorNombre = await db.Categorias
      .Where(c => categoriasDemo.Contains(c.Nombre))
      .ToDictionaryAsync(c => c.Nombre);
    var marcaPorNombre = await db.Marcas
      .Where(m => marcasDemo.Contains(m.Nombre))
      .ToDictionaryAsync(m => m.Nombre);

    var productosDemo = new[]
    {
      new Producto
      {
        Nombre = "Gaseosa Coca-Cola 500 ml",
        CodigoBarras = "7790895000994",
        CategoriaId = categoriaPorNombre["Bebidas"].Id,
        MarcaId = marcaPorNombre["Coca-Cola"].Id,
        PrecioCosto = 900,
        PrecioVenta = 1500,
        StockActual = 20,
        StockMinimo = 5
      },
      new Producto
      {
        Nombre = "Alfajor Bon o Bon",
        CodigoBarras = "7790580114405",
        CategoriaId = categoriaPorNombre["Golosinas"].Id,
        MarcaId = marcaPorNombre["Arcor"].Id,
        PrecioCosto = 500,
        PrecioVenta = 900,
        StockActual = 30,
        StockMinimo = 5
      },
      new Producto
      {
        Nombre = "Papas Lay's clásicas 150 g",
        CodigoBarras = "7790310987654",
        CategoriaId = categoriaPorNombre["Snacks"].Id,
        MarcaId = marcaPorNombre["Pepsi"].Id,
        PrecioCosto = 1200,
        PrecioVenta = 1900,
        StockActual = 15,
        StockMinimo = 5
      }
    };

    var nombresProductosDemo = productosDemo.Select(p => p.Nombre).ToList();
    var productosExistentes = await db.Productos
      .Where(p => nombresProductosDemo.Contains(p.Nombre))
      .Select(p => p.Nombre)
      .ToListAsync();
    db.Productos.AddRange(productosDemo
      .Where(p => !productosExistentes.Contains(p.Nombre)));
    await db.SaveChangesAsync();
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