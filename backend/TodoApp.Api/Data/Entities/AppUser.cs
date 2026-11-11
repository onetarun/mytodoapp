using Microsoft.AspNetCore.Identity;

namespace TodoApp.Api.Data.Entities;

/// <summary>
/// Application user. Identity provides email, password hash and lockout fields.
/// </summary>
public class AppUser : IdentityUser
{
    public const int NameMaxLength = 100;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
