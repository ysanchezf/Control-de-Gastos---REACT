using System.Security.Claims;
using System.Text.Encodings.Web;
using Control_de_Gastos.Data;
using Control_de_Gastos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Control_de_Gastos.Services;

// Espacio de desarrollo sin contraseña. Su base debe ser distinta de la base de cuentas reales.
public sealed class LocalWorkspace
{
    public const string Scheme = "LocalWorkspace";
    public int UserId { get; private set; }

    public async Task InitializeAsync(AppDbContext db)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = await db.Users.SingleOrDefaultAsync(u => u.NormalizedUserName == "LOCAL-WORKSPACE");
        if (user is null)
        {
            // No se genera una contraseña ni una cuenta utilizable por el login normal.
            user = new AppUser { UserName = "local-workspace", NormalizedUserName = "LOCAL-WORKSPACE", DisplayName = "Prueba", IsAdmin = false, IsActive = false };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        UserId = user.Id;
        await db.Categorias.Where(c => c.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(c => c.UserId, user.Id));
        await db.Gastos.Where(g => g.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(g => g.UserId, user.Id));
        var orphaned = await db.Gastos.Where(g => g.UserId == user.Id && !db.Categorias.Any(c => c.Id == g.CategoriaId && c.UserId == user.Id)).ToListAsync();
        if (orphaned.Count > 0)
        {
            var category = await db.Categorias.FirstOrDefaultAsync(c => c.UserId == user.Id && c.Nombre == "Sin categoría");
            if (category is null)
            {
                category = new Categoria { Nombre = "Sin categoría", Descripcion = "Movimientos de ejemplo sin categoría disponible.", UserId = user.Id };
                db.Categorias.Add(category);
                await db.SaveChangesAsync();
            }
            foreach (var expense in orphaned) expense.CategoriaId = category.Id;
            await db.SaveChangesAsync();
        }
        await tx.CommitAsync();
    }
}

// Identidad técnica para reutilizar los filtros de propietario, sin conceder permisos de administrador.
public sealed class LocalWorkspaceHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    LocalWorkspace workspace) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, workspace.UserId.ToString()),
            new Claim(ClaimTypes.Name, "Prueba"),
            new Claim(ClaimTypes.Role, "Usuario")
        }, LocalWorkspace.Scheme);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
