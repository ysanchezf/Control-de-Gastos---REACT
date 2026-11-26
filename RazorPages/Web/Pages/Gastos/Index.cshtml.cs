using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Gastos;
public sealed class IndexModel(IControlGastosApi api) : ApiPageModel
{
    public List<Gasto> Gastos { get; private set; } = [];
    public List<Categoria> Categorias { get; private set; } = [];
    [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }
    public async Task OnGetAsync() => await LoadAsync();
    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (id < 1) return BadRequest();
        try { await api.EliminarGastoAsync(id); Success = "Gasto eliminado."; return RedirectToPage(); }
        catch (ApiException ex) { Error = ex.Message; await LoadAsync(); return Page(); }
    }
    private async Task LoadAsync()
    {
        try {
            Gastos = (await api.GastosAsync()).Where(g => string.IsNullOrWhiteSpace(Buscar) || g.Descripcion.Contains(Buscar, StringComparison.OrdinalIgnoreCase)).OrderByDescending(g => g.Fecha).ThenByDescending(g => g.Id).ToList();
            Categorias = await api.CategoriasAsync();
        } catch (ApiException ex) { Error = ex.Message; }
    }
}
