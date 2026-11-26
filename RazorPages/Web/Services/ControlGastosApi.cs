using System.Net;
using System.Security.Claims;
using System.Net.Http.Json;
using System.Text.Json;
using Bolsillo.Razor.Models;

namespace Bolsillo.Razor.Services;

public sealed class ApiOptions
{
    public string BaseUrl { get; set; } = "";
}
public sealed record TotalResponse(decimal TotalGastos);
public sealed record AccountState(bool FirstAccount);
public sealed record AccountUser(int Id, string Name);
public sealed record AccountSession(AccountUser User, string Ticket);
public sealed class ApiException(string message, HttpStatusCode? status = null) : Exception(message)
{
    public HttpStatusCode? Status { get; } = status;
}
public interface IControlGastosApi
{
    Task<AccountState> AccountStateAsync();
    Task RegisterAsync(RegisterInput input);
    Task<AccountSession> LoginAsync(LoginInput input);
    Task<AccountUser> CurrentAsync(string ticket);
    Task LogoutAsync();
    Task<List<Gasto>> GastosAsync();
    Task<Gasto> GastoAsync(int id);
    Task<List<Categoria>> CategoriasAsync();
    Task<Categoria> CategoriaAsync(int id);
    Task<decimal> TotalAsync();
    Task GuardarGastoAsync(ExpenseInput input, int? id = null);
    Task EliminarGastoAsync(int id);
    Task GuardarCategoriaAsync(CategoryInput input, int? id = null);
    Task EliminarCategoriaAsync(int id);
}
public sealed class ControlGastosApi(HttpClient client, ILogger<ControlGastosApi> logger, IHttpContextAccessor context) : IControlGastosApi
{
    public Task<AccountState> AccountStateAsync() => ReadAsync<AccountState>("cuenta/estado", anonymous: true);
    public async Task RegisterAsync(RegisterInput input) {
        using var response = await WriteResponseAsync(HttpMethod.Post, "cuenta/registro", input, anonymous: true);
    }
    public async Task<AccountSession> LoginAsync(LoginInput input)
    {
        using var response = await WriteResponseAsync(HttpMethod.Post, "cuenta/login", input, anonymous: true);
        var user = await response.Content.ReadFromJsonAsync<AccountUser>() ?? throw new ApiException("No se pudo leer la sesión.");
        var cookies = new CookieContainer();
        if (response.Headers.TryGetValues("Set-Cookie", out var values))
            foreach (var value in values) cookies.SetCookies(client.BaseAddress!, value);
        var ticket = cookies.GetCookies(client.BaseAddress!)[WebSession.ApiCookie]?.Value;
        if (string.IsNullOrEmpty(ticket)) throw new ApiException("No se pudo iniciar la sesión. Intenta nuevamente.");
        return new AccountSession(user, ticket);
    }
    public Task<AccountUser> CurrentAsync(string ticket) => ReadAsync<AccountUser>("cuenta/actual", ticket);
    public Task LogoutAsync() => WriteAsync(HttpMethod.Post, "cuenta/logout", null);

    public Task<List<Gasto>> GastosAsync() => ReadAsync<List<Gasto>>("api/Gastos");
    public Task<Gasto> GastoAsync(int id) => ReadAsync<Gasto>($"api/Gastos/{id}");
    public Task<List<Categoria>> CategoriasAsync() => ReadAsync<List<Categoria>>("api/Categorias");
    public Task<Categoria> CategoriaAsync(int id) => ReadAsync<Categoria>($"api/Categorias/{id}");
    public async Task<decimal> TotalAsync() => (await ReadAsync<TotalResponse>("api/Gastos/total")).TotalGastos;
    public Task GuardarGastoAsync(ExpenseInput input, int? id = null) => WriteAsync(id is null ? HttpMethod.Post : HttpMethod.Put, "api/Gastos" + (id is null ? "" : $"/{id}"), input);
    public Task EliminarGastoAsync(int id) => WriteAsync(HttpMethod.Delete, $"api/Gastos/{id}", null);
    public Task GuardarCategoriaAsync(CategoryInput input, int? id = null) => WriteAsync(id is null ? HttpMethod.Post : HttpMethod.Put, "api/Categorias" + (id is null ? "" : $"/{id}"), input);
    public Task EliminarCategoriaAsync(int id) => WriteAsync(HttpMethod.Delete, $"api/Categorias/{id}", null);

    private async Task<T> ReadAsync<T>(string path, string? ticket = null, bool anonymous = false)
    {
        using var request = Request(HttpMethod.Get, path, ticket, anonymous);
        using var response = await SendAsync(request);
        await EnsureAsync(response);
        try { return await response.Content.ReadFromJsonAsync<T>() ?? throw new ApiException("La API devolvió una respuesta vacía."); }
        catch (JsonException) { throw new ApiException("La API devolvió una respuesta que no se pudo leer."); }
    }
    private async Task WriteAsync(HttpMethod method, string path, object? input)
    {
        using var response = await WriteResponseAsync(method, path, input);
    }
    private async Task<HttpResponseMessage> WriteResponseAsync(HttpMethod method, string path, object? input, bool anonymous = false)
    {
        using var tokenRequest = Request(HttpMethod.Get, "cuenta/antiforgery", anonymous: anonymous);
        using var tokenResponse = await SendAsync(tokenRequest);
        await EnsureAsync(tokenResponse);
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        if (string.IsNullOrWhiteSpace(token?.Token)) throw new ApiException("No se pudo obtener la protección de la API. Intenta nuevamente.");
        // Las cookies pertenecen solo a esta operación; no se comparten entre visitantes.
        var cookies = new CookieContainer();
        var session = anonymous ? null : context.HttpContext?.User.FindFirstValue(WebSession.TicketClaim);
        if (!string.IsNullOrEmpty(session)) cookies.SetCookies(client.BaseAddress!, WebSession.ApiCookie + "=" + session);
        var uri = new Uri(client.BaseAddress!, "cuenta/antiforgery");
        if (tokenResponse.Headers.TryGetValues("Set-Cookie", out var values))
            foreach (var value in values) cookies.SetCookies(uri, value);
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", token.Token);
        request.Headers.Add("Cookie", cookies.GetCookieHeader(new Uri(client.BaseAddress!, path)));
        if (input is not null) request.Content = JsonContent.Create(input);
        var response = await SendAsync(request);
        try { await EnsureAsync(response); return response; }
        catch { response.Dispose(); throw; }
    }
    private HttpRequestMessage Request(HttpMethod method, string path, string? ticket = null, bool anonymous = false)
    {
        var request = new HttpRequestMessage(method, path);
        var session = ticket ?? (anonymous ? null : context.HttpContext?.User.FindFirstValue(WebSession.TicketClaim));
        if (!string.IsNullOrEmpty(session)) request.Headers.Add("Cookie", WebSession.ApiCookie + "=" + session);
        return request;
    }
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        try { return await client.SendAsync(request); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "No se pudo completar la solicitud a la API");
            throw new ApiException("No se pudo conectar con la API. Comprueba que esté ejecutándose en el puerto configurado y vuelve a intentar.");
        }
    }
    private static async Task EnsureAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        string message = response.StatusCode switch {
            HttpStatusCode.Unauthorized => "Tu sesión terminó. Vuelve a iniciar sesión.",
            HttpStatusCode.TooManyRequests => "Demasiados intentos. Espera un minuto antes de volver a intentar.",
            HttpStatusCode.NotFound => "El registro ya no existe. Actualiza la página.",
            HttpStatusCode.BadRequest => "Revisa los datos del formulario y vuelve a intentar.",
            _ => "No se pudo completar la operación. Intenta nuevamente."
        };
        try {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (json.RootElement.TryGetProperty("message", out var text)) message = text.GetString() ?? message;
            else if (json.RootElement.TryGetProperty("errors", out var errors))
                message = string.Join(" ", errors.EnumerateObject().SelectMany(e => e.Value.EnumerateArray().Select(x => x.GetString())));
        } catch (JsonException) { }
        throw new ApiException(message, response.StatusCode);
    }
    private sealed record TokenResponse(string Token);
}
