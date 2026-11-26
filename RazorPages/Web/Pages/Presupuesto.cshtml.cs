using System.ComponentModel.DataAnnotations;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages;
public sealed class PresupuestoModel(IControlGastosApi api, IConfiguration configuration) : ApiPageModel
{
    [BindProperty, Range(typeof(decimal), "1", "999999999", ErrorMessage = "El presupuesto debe estar entre 1 y 999999999.")]
    public decimal Limite { get; set; }
    public decimal Gastado { get; private set; }
    public decimal Disponible => Limite - Gastado;
    public decimal Porcentaje => Limite > 0 ? Math.Min(Gastado / Limite * 100, 100) : 0;
    public async Task OnGetAsync()
    {
        Limite = configuration.GetValue<decimal>("AppSettings:PresupuestoSugerido", 15000);
        await LoadAsync();
    }
    public async Task<IActionResult> OnPostAsync() { await LoadAsync(); return Page(); }
    private async Task LoadAsync()
    {
        try {
            var today = DateTime.Today;
            Gastado = (await api.GastosAsync()).Where(g => g.Fecha.Year == today.Year && g.Fecha.Month == today.Month).Sum(g => g.Monto);
        } catch (ApiException ex) { Error = ex.Message; }
    }
}
