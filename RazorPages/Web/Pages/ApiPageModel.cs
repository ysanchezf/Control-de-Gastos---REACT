using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Bolsillo.Razor.Pages;
public abstract class ApiPageModel : PageModel
{
    public string? Error { get; set; }
    [TempData] public string? Success { get; set; }
}
