using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.ViewModels;

public class CompraCrearVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un proveedor")]
    public int ProveedorId { get; set; }

    public string? ProveedorNombre { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime Fecha { get; set; } = DateTime.Today;
}