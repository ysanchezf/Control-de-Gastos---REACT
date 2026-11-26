using System.Security.Claims;
using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Cuenta;
public sealed class LoginModel(IControlGastosApi api) : ApiPageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Resumen") : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try {
            var session = await api.LoginAsync(Input);
            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, session.User.Id.ToString()),
                new Claim(ClaimTypes.Name, session.User.Name),
                new Claim(WebSession.TicketClaim, session.Ticket)
            }, WebSession.Scheme);
            await HttpContext.SignInAsync(WebSession.Scheme, new ClaimsPrincipal(identity), new AuthenticationProperties {
                IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            });
            // La cookie de la API también permite abrir React con la misma cuenta y host.
            Response.Cookies.Append(WebSession.ApiCookie, session.Ticket, WebSession.ApiCookieOptions(Request));
            return Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl!) : RedirectToPage("/Resumen");
        } catch (ApiException ex) {
            Error = ex.Message;
            Input.Password = "";
            ModelState.Remove("Input.Password");
            return Page();
        }
    }
}
