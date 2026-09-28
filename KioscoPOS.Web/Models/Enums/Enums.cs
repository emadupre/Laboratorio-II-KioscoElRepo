namespace KioscoPOS.Web.Models.Enums;

public enum Turno
{
  Manana = 1,
  Tarde = 2,
  Noche = 3
}

public enum MedioPago
{
  Efectivo = 1,
  Debito = 2,
  Credito = 3,
  Transferencia = 4,
  QR = 5
}

public enum EstadoVenta
{
  Confirmada = 1,
  Anulada = 2
}

public enum EstadoCompra
{
  Borrador = 1,
  Confirmada = 2,
  Anulada = 3
}

public enum TipoMovimientoCaja
{
  Ingreso = 1,
  Retiro = 2
}

public enum TipoMovimientoStock
{
  Apertura = 1,
  Venta = 2,
  Compra = 3,
  AnulacionVenta = 4,
  AnulacionCompra = 5,
  AjusteManual = 6
}

public static class Roles
{
  public const string Administrador = "Administrador";
  public const string Cajero = "Cajero";
}
