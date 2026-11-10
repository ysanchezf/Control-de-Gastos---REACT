using System.ComponentModel.DataAnnotations;
namespace Control_de_Gastos.Models;
public sealed class ExpenseInput
{
    [Required(ErrorMessage = "Escribe una descripción."), StringLength(150, MinimumLength = 2, ErrorMessage = "Usa entre 2 y 150 caracteres.")]
    public string Descripcion { get; set; } = "";
    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "El monto debe ser mayor que cero y menor que mil millones.")]
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría.")]
    public int CategoriaId { get; set; }
}
