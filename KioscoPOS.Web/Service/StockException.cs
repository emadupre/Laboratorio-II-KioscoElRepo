namespace KioscoPOS.Web.Services;

public class StockException : Exception
{
    public StockException(string mensaje) : base(mensaje) { }
}