using System.Globalization;
using System.Text;
using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages;
public sealed class ReportesModel(IControlGastosApi api) : ApiPageModel
{
    [BindProperty(SupportsGet = true)] public string? Mes { get; set; } = DateTime.Today.ToString("yyyy-MM");
    public List<Gasto> Gastos { get; private set; } = [];
    public List<Categoria> Categorias { get; private set; } = [];
    public decimal Total => Gastos.Sum(g => g.Monto);
    public async Task OnGetAsync() => await LoadAsync();
    public async Task<IActionResult> OnGetExportarAsync()
    {
        await LoadAsync();
        if (Error is not null || !ModelState.IsValid) return Page();
        var csv = new StringBuilder("Fecha,Descripción,Categoría,Monto\r\n");
        foreach (var g in Gastos) csv.AppendLine(string.Join(",", g.Fecha.ToString("yyyy-MM-dd"), Cell(g.Descripcion), Cell(Categorias.FirstOrDefault(c => c.Id == g.CategoriaId)?.Nombre ?? ""), g.Monto.ToString("0.00", CultureInfo.InvariantCulture)));
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"bolsillo-{Mes}.csv");
    }
    private async Task LoadAsync()
    {
        Mes ??= DateTime.Today.ToString("yyyy-MM");
        if (!DateTime.TryParseExact(Mes, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month)) {
            ModelState.AddModelError(nameof(Mes), "Selecciona un mes válido."); return;
        }
        try {
            Gastos = (await api.GastosAsync()).Where(g => g.Fecha.Year == month.Year && g.Fecha.Month == month.Month).OrderBy(g => g.Fecha).ToList();
            Categorias = await api.CategoriasAsync();
        } catch (ApiException ex) { Error = ex.Message; }
    }
    private static string Cell(string value)
    {
        // Evita que una descripción se interprete como fórmula al abrir el CSV.
        if (value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+') || value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@') || value.StartsWith('\t') || value.StartsWith('\r') || value.StartsWith('\n')) value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
