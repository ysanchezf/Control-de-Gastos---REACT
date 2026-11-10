using System.Text.Json.Serialization;
namespace Control_de_Gastos.Models;
public sealed class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = "";
    public string NormalizedUserName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    [JsonIgnore] public string PasswordHash { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
