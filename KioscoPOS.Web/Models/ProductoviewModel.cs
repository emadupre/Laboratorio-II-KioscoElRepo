using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
public class ProductoViewModel
{
  public int Id { get; set; }

  [Required(ErrorMessage = "El nombre es obligatorio"), StringLength(120)]
  public string Nombre { get; set; } = string.Empty;

  [StringLength(50)]
  [Display(Name = "Código de barras")]
  public string? CodigoBarras { get; set; }

  [Range(0, 99999999, ErrorMessage = "Valor inválido")]
  [Display(Name = "Precio de costo")]
  public decimal PrecioCosto { get; set; }

  [Range(0, 99999999, ErrorMessage = "Valor inválido")]
  [Display(Name = "Precio de venta")]
  public decimal PrecioVenta { get; set; }

  [Range(0, int.MaxValue, ErrorMessage = "Valor inválido")]
  [Display(Name = "Stock actual")]

  public int StockActual { get; set; }

  [Range(0, int.MaxValue, ErrorMessage = "Valor inválido")]
  [Display(Name = "Stock mínimo")]
  public int StockMinimo { get; set; }

  [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una categoría")]
  [Display(Name = "Categoría")]
  public int CategoriaId { get; set; }
  public string? CategoriaNombre { get; set; }

  [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una marca")]
  [Display(Name = "Marca")]
  public int MarcaId { get; set; }
  public string? MarcaNombre{ get; set; }

  public string? ImagenPath { get; set; }

  [Display(Name = "Imagen")]
  public IFormFile? ImagenArchivo { get; set; }
}