using System.ComponentModel.DataAnnotations;
namespace Control_de_Gastos.Models;
public sealed class LoginInput
{
    [Required(ErrorMessage = "Escribe tu usuario."), StringLength(40)]
    public string UserName { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu contraseña."), StringLength(128)]
    public string Password { get; set; } = "";
}
public sealed class RegisterInput
{
    [Required(ErrorMessage = "Escribe tu nombre."), StringLength(80, MinimumLength = 2)]
    public string DisplayName { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu usuario."), RegularExpression(@"^[a-zA-Z0-9._-]{3,40}$", ErrorMessage = "El usuario debe tener entre 3 y 40 letras, números, puntos, guiones o guiones bajos.")]
    public string UserName { get; set; } = "";
    [Required(ErrorMessage = "Escribe una contraseña."), StringLength(128, MinimumLength = 10, ErrorMessage = "La contraseña debe tener entre 10 y 128 caracteres.")]
    public string Password { get; set; } = "";
    public bool IncludeLegacyData { get; set; }
}
