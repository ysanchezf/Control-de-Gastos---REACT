using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
namespace Bolsillo.Razor.Pages;
public sealed class ResumenModel(IControlGastosApi api) : ApiPageModel
{
    public List<Gasto> Gastos { get; private set; } = [];
    public List<Categoria> Categorias { get; private set; } = [];
    public decimal Total { get; private set; }
    public async Task OnGetAsync()
    {
        try {
            var gastos = api.GastosAsync(); var categorias = api.CategoriasAsync(); var total = api.TotalAsync();
            await Task.WhenAll(gastos, categorias, total);
            Gastos = await gastos; Categorias = await categorias; Total = await total;
        } catch (ApiException ex) { Error = ex.Message; }
    }
}
