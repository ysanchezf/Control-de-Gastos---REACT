using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
namespace Bolsillo.Razor.Services;
public static class WebSession
{
    public const string Scheme = "Bolsillo.Razor.Accounts";
    public const string TicketClaim = "bolsillo:api-ticket";
    public const string ApiCookie = "Bolsillo.Api.Session";
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var ticket = context.Principal?.FindFirstValue(TicketClaim);
        try {
            if (string.IsNullOrEmpty(ticket)) { context.RejectPrincipal(); return; }
            var api = context.HttpContext.RequestServices.GetRequiredService<IControlGastosApi>();
            var user = await api.CurrentAsync(ticket);
            if (context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier) != user.Id.ToString()) context.RejectPrincipal();
        } catch (ApiException ex) when (ex.Status is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden) {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(Scheme);
            context.HttpContext.Response.Cookies.Delete(ApiCookie);
        } catch (ApiException) {
            // Una interrupción de la API se muestra como error; no destruye la sesión local.
        }
    }
    public static CookieOptions ApiCookieOptions(HttpRequest request) => new() {
        HttpOnly = true, Secure = request.IsHttps, SameSite = SameSiteMode.Lax, Path = "/"
    };
}
