using KioscoPOS.Web.Models;
using KioscoPOS.Web.Services;

namespace KioscoPOS.Web.ViewModels;

public class CajaPanelVM
{
    // null si el usuario no tiene una caja abierta
    public Caja? Caja { get; set; }

    // solo viene cargado cuando hay caja abierta
    public ResumenCaja? Resumen { get; set; }
}