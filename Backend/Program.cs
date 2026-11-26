using Control_de_Gastos.Data;
using Control_de_Gastos.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Control_de_Gastos.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
// El backend conserva compatibilidad con React y añade cuentas para Razor Pages.
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Este proyecto académico local solo se ejecuta en Development.");
LocalDataMigration.Prepare(builder.Environment.ContentRootPath, builder.Configuration.GetConnectionString("DemoConnection"));
// El backend puede servir también la versión compilada de React.
var reactRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "React", "wwwroot"));
if (!Directory.Exists(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")) && Directory.Exists(reactRoot))
{
    builder.Environment.WebRootPath = reactRoot;
    builder.Environment.WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(reactRoot);
}
// El filtro antiforgery MVC requiere los servicios de ViewFeatures.
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
});
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DemoConnection")));
builder.Services.AddSingleton<LocalWorkspace>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddAuthentication("Bolsillo")
    .AddPolicyScheme("Bolsillo", null, options => options.ForwardDefaultSelector = context =>
        context.Request.Cookies.ContainsKey(AccountAuthentication.CookieName) ? AccountAuthentication.Scheme : LocalWorkspace.Scheme)
    .AddCookie(AccountAuthentication.Scheme, options => {
        options.Cookie.Name = AccountAuthentication.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = false;
        options.Events.OnValidatePrincipal = AccountAuthentication.ValidateAsync;
        options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    })
    .AddScheme<AuthenticationSchemeOptions, LocalWorkspaceHandler>(LocalWorkspace.Scheme, _ => { });
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.AddPolicy("accounts", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions {
            PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
});
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(o => o.HeaderName = "X-CSRF-TOKEN");
Directory.CreateDirectory("App_Data/keys");
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("App_Data/keys")).SetApplicationName("Bolsillo.React");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<LocalWorkspace>().InitializeAsync(db);
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.MapGet("/cuenta/antiforgery", (HttpContext context, IAntiforgery antiforgery) => {
    context.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken });
}).RequireAuthorization();
app.MapControllers();
app.MapFallbackToFile("build/index.html");
app.Run();
