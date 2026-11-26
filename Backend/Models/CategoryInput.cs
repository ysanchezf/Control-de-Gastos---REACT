using System.ComponentModel.DataAnnotations;
namespace Control_de_Gastos.Models;
public sealed class CategoryInput
{
    [Required(ErrorMessage = "Escribe un nombre."), StringLength(80, MinimumLength = 2, ErrorMessage = "Usa entre 2 y 80 caracteres.")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Escribe una descripción."), StringLength(250, ErrorMessage = "Usa hasta 250 caracteres.")]
    public string Descripcion { get; set; } = "";
}
