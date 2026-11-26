using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Gastos;
public sealed class DetalleModel(IControlGastosApi api) : ApiPageModel
{
    public Gasto? Gasto { get; private set; }
    public string Categoria { get; private set; } = "";
    public async Task<IActionResult> OnGetAsync(int id)
    {
        try {
            Gasto = await api.GastoAsync(id);
            Categoria = (await api.CategoriasAsync()).FirstOrDefault(c => c.Id == Gasto.CategoriaId)?.Nombre ?? "Sin categoría";
        } catch (ApiException ex) {
            if (ex.Status == System.Net.HttpStatusCode.NotFound) return NotFound();
            Error = ex.Message;
        }
        return Page();
    }
}
