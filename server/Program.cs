using Control_de_Gastos.Data;
using Control_de_Gastos.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Este artefacto conserva el espacio de demostración del proyecto anterior.
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Este proyecto académico sin login solo se ejecuta en Development.");
// El filtro antiforgery MVC requiere los servicios de ViewFeatures.
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
});
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DemoConnection")));
builder.Services.AddSingleton<LocalWorkspace>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddAuthentication(LocalWorkspace.Scheme)
    .AddScheme<AuthenticationSchemeOptions, LocalWorkspaceHandler>(LocalWorkspace.Scheme, _ => { });
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
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapGet("/cuenta/antiforgery", (HttpContext context, IAntiforgery antiforgery) => {
    context.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken });
}).RequireAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();
