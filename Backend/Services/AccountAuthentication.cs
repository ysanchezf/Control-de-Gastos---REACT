using System.Security.Claims;
using Control_de_Gastos.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
namespace Control_de_Gastos.Services;
public static class AccountAuthentication
{
    public const string Scheme = "Bolsillo.Accounts";
    public const string CookieName = "Bolsillo.Api.Session";
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        if (!int.TryParse(id, out var userId) || !await db.Users.AnyAsync(u => u.Id == userId && u.IsActive))
            context.RejectPrincipal();
    }
}
