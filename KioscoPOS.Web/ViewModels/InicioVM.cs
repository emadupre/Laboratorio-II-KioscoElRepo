namespace KioscoPOS.Web.ViewModels;

public class InicioVM
{
  public int TotalProductos { get; set; }
  public int ProductosStockBajo { get; set; }
  public int VentasHoy { get; set; }
  public decimal TotalVentasHoy { get; set; }
  public bool TieneCajaAbierta { get; set; }
  public bool EsAdmin { get; set; }
}