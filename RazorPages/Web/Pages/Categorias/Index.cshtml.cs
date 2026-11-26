using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Categorias;
public sealed class IndexModel(IControlGastosApi api) : ApiPageModel
{
    public List<Categoria> Categorias { get; private set; } = [];
    public async Task OnGetAsync() => await LoadAsync();
    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (id < 1) return BadRequest();
        try { await api.EliminarCategoriaAsync(id); Success = "Categoría eliminada."; return RedirectToPage(); }
        catch (ApiException ex) { Error = ex.Message; await LoadAsync(); return Page(); }
    }
    private async Task LoadAsync()
    {
        try { Categorias = (await api.CategoriasAsync()).OrderBy(c => c.Nombre).ToList(); }
        catch (ApiException ex) { Error = ex.Message; }
    }
}
