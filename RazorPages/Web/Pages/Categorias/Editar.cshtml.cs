using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Categorias;
public sealed class EditarModel(IControlGastosApi api) : ApiPageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public CategoryInput Input { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        if (Id is null) return Page();
        try {
            var c = await api.CategoriaAsync(Id.Value);
            Input = new() { Nombre = c.Nombre, Descripcion = c.Descripcion };
        } catch (ApiException ex) {
            if (ex.Status == System.Net.HttpStatusCode.NotFound) return NotFound();
            Error = ex.Message;
        }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await api.GuardarCategoriaAsync(Input, Id); Success = "Categoría guardada."; return RedirectToPage("Index"); }
        catch (ApiException ex) { Error = ex.Message; return Page(); }
    }
}
