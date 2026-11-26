using Bolsillo.Razor.Models;
using Bolsillo.Razor.Services;
using Microsoft.AspNetCore.Mvc;
namespace Bolsillo.Razor.Pages.Cuenta;
public sealed class RegistroModel(IControlGastosApi api) : ApiPageModel
{
    [BindProperty] public RegisterInput Input { get; set; } = new() { IncludeLegacyData = true };
    public bool FirstAccount { get; private set; }
    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/Resumen");
        await LoadAsync(); return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        await LoadAsync();
        if (!ModelState.IsValid || Error is not null) return Page();
        try {
            await api.RegisterAsync(Input);
            Success = "Cuenta creada. Ya puedes iniciar sesión.";
            return RedirectToPage("Login");
        } catch (ApiException ex) {
            Error = ex.Message;
            Input.Password = Input.ConfirmPassword = "";
            ModelState.Remove("Input.Password"); ModelState.Remove("Input.ConfirmPassword");
            return Page();
        }
    }
    private async Task LoadAsync()
    {
        try { FirstAccount = (await api.AccountStateAsync()).FirstAccount; }
        catch (ApiException ex) { Error = ex.Message; }
    }
}
