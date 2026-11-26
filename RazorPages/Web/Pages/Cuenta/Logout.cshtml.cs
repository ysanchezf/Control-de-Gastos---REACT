using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Cuenta;
public sealed class LogoutModel(IControlGastosApi api) : ApiPageModel
{
    public IActionResult OnGet() => RedirectToPage("/Resumen");
    public async Task<IActionResult> OnPostAsync()
    {
        try { await api.LogoutAsync(); } catch (ApiException) { }
        await HttpContext.SignOutAsync(WebSession.Scheme);
        Response.Cookies.Delete(WebSession.ApiCookie);
        return RedirectToPage("/Cuenta/Login");
    }
}
