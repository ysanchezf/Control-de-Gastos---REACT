using Microsoft.Extensions.Configuration;
namespace Bolsillo.Razor.Pages;
public sealed class IndexModel(IConfiguration configuration) : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
{
    public string NombreAplicacion { get; private set; } = "";
    public void OnGet() => NombreAplicacion = configuration["AppSettings:NombreAplicacion"] ?? "bolsillo";
}
