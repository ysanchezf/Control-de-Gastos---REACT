using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Gastos;
public sealed class EditarModel(IControlGastosApi api) : ApiPageModel
{
    [BindProperty(SupportsGet = true)] public int? Id { get; set; }
    [BindProperty] public ExpenseInput Input { get; set; } = new();
    public List<Categoria> Categorias { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync()
    {
        try {
            Categorias = await api.CategoriasAsync();
            if (Id is not null) {
                var g = await api.GastoAsync(Id.Value);
                Input = new() { Descripcion = g.Descripcion, Monto = g.Monto, Fecha = g.Fecha, CategoriaId = g.CategoriaId };
            }
        } catch (ApiException ex) {
            if (ex.Status == System.Net.HttpStatusCode.NotFound) return NotFound();
            Error = ex.Message;
        }
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        try {
            Categorias = await api.CategoriasAsync();
            if (!ModelState.IsValid) return Page();
            await api.GuardarGastoAsync(Input, Id);
            Success = "Gasto guardado.";
            return RedirectToPage("Index");
        } catch (ApiException ex) { Error = ex.Message; return Page(); }
    }
}
