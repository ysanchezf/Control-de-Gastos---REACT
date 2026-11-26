using System.Globalization;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Esta demostración compartida solo se ejecuta en Development.");
builder.Services.AddRazorPages(options => options.Conventions.ConfigureFilter(
    new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute { NoStore = true, Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None }));
builder.Services.AddOptions<ApiOptions>().Bind(builder.Configuration.GetSection("Api"))
    .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out var url) &&
        (url.Scheme == "http" || url.Scheme == "https"), "Configura Api:BaseUrl con una URL HTTP válida.")
    .ValidateOnStart();
builder.Services.AddHttpClient<IControlGastosApi, ControlGastosApi>((services, client) => {
    var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
Directory.CreateDirectory("App_Data/keys");
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo("App_Data/keys")).SetApplicationName("Bolsillo.Razor.Web");
builder.Services.AddAntiforgery(o => o.Cookie.Name = "Bolsillo.Razor.Web.Csrf");
var app = builder.Build();
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("es-DO").AddSupportedCultures("es-DO").AddSupportedUICultures("es-DO"));
app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
