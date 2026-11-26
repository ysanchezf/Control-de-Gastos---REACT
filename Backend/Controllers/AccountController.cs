using System.Security.Claims;
using Control_de_Gastos.Data;
using Control_de_Gastos.Models;
using Control_de_Gastos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
namespace Control_de_Gastos.Controllers;
[ApiController, Route("cuenta")]
public sealed class AccountController(AppDbContext db, IPasswordHasher<AppUser> hasher, LocalWorkspace workspace) : ControllerBase
{
    [HttpGet("estado"), AllowAnonymous]
    public async Task<IActionResult> State() => Ok(new { firstAccount = !await db.Users.AnyAsync(u => u.IsActive) });

    [HttpPost("registro"), AllowAnonymous, EnableRateLimiting("accounts")]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        var normalized = input.UserName.Trim().ToUpperInvariant();
        await using var tx = await db.Database.BeginTransactionAsync();
        if (await db.Users.AnyAsync(u => u.NormalizedUserName == normalized))
            return Conflict(new { message = "No se pudo registrar ese usuario. Elige otro nombre de usuario." });
        var first = !await db.Users.AnyAsync(u => u.IsActive);
        var user = new AppUser { UserName = input.UserName.Trim(), NormalizedUserName = normalized, DisplayName = input.DisplayName.Trim() };
        user.PasswordHash = hasher.HashPassword(user, input.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        if (first && input.IncludeLegacyData) {
            await db.Categorias.Where(c => c.UserId == workspace.UserId).ExecuteUpdateAsync(s => s.SetProperty(c => c.UserId, user.Id));
            await db.Gastos.Where(g => g.UserId == workspace.UserId).ExecuteUpdateAsync(s => s.SetProperty(g => g.UserId, user.Id));
        }
        await tx.CommitAsync();
        return StatusCode(201, new { message = "Cuenta creada. Ya puedes iniciar sesión." });
    }
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("accounts")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        var normalized = input.UserName.Trim().ToUpperInvariant();
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = await db.Users.SingleOrDefaultAsync(u => u.NormalizedUserName == normalized);
        var now = DateTime.UtcNow;
        if (user is null || !user.IsActive || user.LockedUntilUtc > now) {
            // También realiza un hash para no responder inmediatamente a usuarios inexistentes.
            if (user is null) hasher.HashPassword(new AppUser(), input.Password);
            return Unauthorized(new { message = "Usuario o contraseña incorrectos, o acceso bloqueado temporalmente." });
        }
        if (user.LockedUntilUtc is not null) { user.FailedAttempts = 0; user.LockedUntilUtc = null; }
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (result == PasswordVerificationResult.Failed) {
            user.FailedAttempts++;
            if (user.FailedAttempts >= 5) user.LockedUntilUtc = now.AddMinutes(15);
            await db.SaveChangesAsync(); await tx.CommitAsync();
            return Unauthorized(new { message = "Usuario o contraseña incorrectos, o acceso bloqueado temporalmente." });
        }
        if (result == PasswordVerificationResult.SuccessRehashNeeded) user.PasswordHash = hasher.HashPassword(user, input.Password);
        user.FailedAttempts = 0; user.LockedUntilUtc = null;
        await db.SaveChangesAsync(); await tx.CommitAsync();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.DisplayName), new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User") };
        await HttpContext.SignInAsync(AccountAuthentication.Scheme, new ClaimsPrincipal(new ClaimsIdentity(claims, AccountAuthentication.Scheme)), new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) });
        return Ok(new { id = user.Id, name = user.DisplayName });
    }
    [HttpGet("actual"), Authorize(AuthenticationSchemes = AccountAuthentication.Scheme)]
    public IActionResult Current() => Ok(new { id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), name = User.Identity!.Name });
    [HttpPost("logout"), Authorize(AuthenticationSchemes = AccountAuthentication.Scheme)]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(AccountAuthentication.Scheme); return NoContent(); }
}
