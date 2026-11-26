using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bolsillo.Razor.Models;

namespace Bolsillo.Razor.Services;

public sealed class ApiOptions
{
    public string BaseUrl { get; set; } = "";
}
public sealed record TotalResponse(decimal TotalGastos);
public sealed class ApiException(string message, HttpStatusCode? status = null) : Exception(message)
{
    public HttpStatusCode? Status { get; } = status;
}
public interface IControlGastosApi
{
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
public sealed class ControlGastosApi(HttpClient client, ILogger<ControlGastosApi> logger) : IControlGastosApi
{
    public Task<List<Gasto>> GastosAsync() => ReadAsync<List<Gasto>>("api/Gastos");
    public Task<Gasto> GastoAsync(int id) => ReadAsync<Gasto>($"api/Gastos/{id}");
    public Task<List<Categoria>> CategoriasAsync() => ReadAsync<List<Categoria>>("api/Categorias");
    public Task<Categoria> CategoriaAsync(int id) => ReadAsync<Categoria>($"api/Categorias/{id}");
    public async Task<decimal> TotalAsync() => (await ReadAsync<TotalResponse>("api/Gastos/total")).TotalGastos;
    public Task GuardarGastoAsync(ExpenseInput input, int? id = null) => WriteAsync(id is null ? HttpMethod.Post : HttpMethod.Put, "api/Gastos" + (id is null ? "" : $"/{id}"), input);
    public Task EliminarGastoAsync(int id) => WriteAsync(HttpMethod.Delete, $"api/Gastos/{id}", null);
    public Task GuardarCategoriaAsync(CategoryInput input, int? id = null) => WriteAsync(id is null ? HttpMethod.Post : HttpMethod.Put, "api/Categorias" + (id is null ? "" : $"/{id}"), input);
    public Task EliminarCategoriaAsync(int id) => WriteAsync(HttpMethod.Delete, $"api/Categorias/{id}", null);

    private async Task<T> ReadAsync<T>(string path)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await SendAsync(request);
        await EnsureAsync(response);
        try { return await response.Content.ReadFromJsonAsync<T>() ?? throw new ApiException("La API devolvió una respuesta vacía."); }
        catch (JsonException) { throw new ApiException("La API devolvió una respuesta que no se pudo leer."); }
    }
    private async Task WriteAsync(HttpMethod method, string path, object? input)
    {
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Get, "cuenta/antiforgery");
        using var tokenResponse = await SendAsync(tokenRequest);
        await EnsureAsync(tokenResponse);
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        if (string.IsNullOrWhiteSpace(token?.Token)) throw new ApiException("No se pudo obtener la protección de la API. Intenta nuevamente.");
        // Las cookies pertenecen solo a esta operación; no se comparten entre visitantes.
        var cookies = new CookieContainer();
        var uri = new Uri(client.BaseAddress!, "cuenta/antiforgery");
        if (tokenResponse.Headers.TryGetValues("Set-Cookie", out var values))
            foreach (var value in values) cookies.SetCookies(uri, value);
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", token.Token);
        request.Headers.Add("Cookie", cookies.GetCookieHeader(new Uri(client.BaseAddress!, path)));
        if (input is not null) request.Content = JsonContent.Create(input);
        using var response = await SendAsync(request);
        await EnsureAsync(response);
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
