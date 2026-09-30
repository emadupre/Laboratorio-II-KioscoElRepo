using KioscoPOS.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KioscoPOS.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
  {

  }
  public DbSet<Categoria> Categorias => Set<Categoria>();
  public DbSet<Producto> Productos => Set<Producto>();
  public DbSet<Proveedor> Proveedores => Set<Proveedor>();
  public DbSet<Compra> Compras => Set<Compra>();
  public DbSet<DetalleCompra> DetalleCompras => Set<DetalleCompra>();
  public DbSet<Venta> Ventas => Set<Venta>();
  public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
  public DbSet<Caja> Cajas => Set<Caja>();
  public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
  public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

  protected override void OnModelCreating(ModelBuilder b)
  {
    base.OnModelCreating(b);

    //CATALAGO

    b.Entity<Producto>()
    .HasOne(p => p.Categoria)
    .WithMany(c => c.Productos)
    .HasForeignKey(p => p.CategoriaId)
    .OnDelete(DeleteBehavior.Restrict);

    //indice unico en el codigo de barras, 
    // en el mysql permite los multiples null en un indice unique, 
    // así que los prod sin cod no chocan entre sí.

    b.Entity<Producto>()
    .HasIndex(p => p.CodigoBarras)
    .IsUnique();

    //token
    b.Entity<Producto>()
    .Property(p => p.RowVersion)
    .IsRowVersion();

    // check para el stock nunca negativo
    b.Entity<Producto>()
    .ToTable(t => t.HasCheckConstraint("CK_Producto_StockNoNegativo", "`StockActual` >= 0"));

    // COMPRAS

    b.Entity<Compra>()
    .HasOne(c => c.Proveedor)
    .WithMany(p => p.Compras)
    .HasForeignKey(c => c.ProveedorId)
    .OnDelete(DeleteBehavior.Restrict);

    b.Entity<Compra>()
    .HasOne(c => c.Usuario)
    .WithMany(u => u.Compras)
    .HasForeignKey(c => c.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);

    b.Entity<DetalleCompra>()
    .HasOne(d => d.Compra)
    .WithMany(c => c.Detalles)
    .HasForeignKey(d => d.CompraId)
    .OnDelete(DeleteBehavior.Cascade);

    b.Entity<DetalleCompra>()
    .HasOne(d => d.Producto)
    .WithMany(p => p.DetalleCompras)
    .HasForeignKey(d => d.ProductoId)
    .OnDelete(DeleteBehavior.Restrict);

    //VENTAS

    b.Entity<Venta>()
    .HasOne(v => v.Usuario)
    .WithMany(u => u.Ventas)
    .HasForeignKey(v => v.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);

    b.Entity<Venta>()
    .HasOne(v => v.Caja)
    .WithMany(c => c.Ventas)
    .HasForeignKey(v => v.CajaId)
    .OnDelete(DeleteBehavior.Restrict);

    // aca venta -> usuario que anuló, es opcional.
    b.Entity<Venta>()
    .HasOne<ApplicationUser>()
    .WithMany()
    .HasForeignKey(v => v.AnuladaPorId)
    .OnDelete(DeleteBehavior.Restrict);

    b.Entity<DetalleVenta>()
    .HasOne(d => d.Venta)
    .WithMany(v => v.Detalles)
    .HasForeignKey(d => d.VentaId)
    .OnDelete(DeleteBehavior.Cascade);

    b.Entity<DetalleVenta>()
    .HasOne(d => d.Producto)
    .WithMany(p => p.DetalleVentas)
    .HasForeignKey(d => d.ProductoId)
    .OnDelete(DeleteBehavior.Restrict);

    //Caja

    b.Entity<Caja>()
    .HasOne(c => c.Usuario)
    .WithMany(u => u.Cajas)
    .HasForeignKey(c => c.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);

    b.Entity<MovimientoCaja>()
    .HasOne(m => m.Caja)
    .WithMany(c => c.Movimientos)
    .HasForeignKey(m => m.CajaId)
    .OnDelete(DeleteBehavior.Cascade);

    b.Entity<MovimientoCaja>()
    .HasOne(m => m.Usuario)
    .WithMany()
    .HasForeignKey(m => m.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);

    //Aauditoria del stock

    b.Entity<MovimientoStock>()
    .HasOne(m => m.Producto)
    .WithMany(p => p.Movientos)
    .HasForeignKey(m => m.ProductoId)
    .OnDelete(DeleteBehavior.Restrict);

    b.Entity<MovimientoStock>()
    .HasOne(m => m.Usuario)
    .WithMany()
    .HasForeignKey(m => m.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);

  }
}