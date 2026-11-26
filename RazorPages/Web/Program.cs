using Microsoft.AspNetCore.Authentication.Cookies;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Esta aplicación académica local solo se ejecuta en Development.");
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(WebSession.Scheme).AddCookie(WebSession.Scheme, options => {
    options.Cookie.Name = "Bolsillo.Razor.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = false;
    options.Events.OnValidatePrincipal = WebSession.ValidateAsync;
});
builder.Services.AddAuthorization();
builder.Services.AddRazorPages(options => {
    options.Conventions.AuthorizeFolder("/");
    foreach (var page in new[] { "/Index", "/Cuenta/Login", "/Cuenta/Registro", "/Error" })
        options.Conventions.AllowAnonymousToPage(page);
    options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.ResponseCacheAttribute {
        NoStore = true, Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None
    });
});
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
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
