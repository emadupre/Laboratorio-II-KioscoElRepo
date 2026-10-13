using System.ComponentModel.DataAnnotations;

namespace KioscoPOS.Web.ViewModels;

public class AjusteStockVM
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un producto")]
    public int ProductoId { get; set; }

    public string? ProductoNombre { get; set; }

    [Required]
    public string Operacion { get; set; } = "disminuir"; // aumentar | disminuir

    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a cero")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio")]
    [StringLength(250)]
    public string Motivo { get; set; } = string.Empty;
}