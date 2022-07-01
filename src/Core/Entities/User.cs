namespace Hippo.Core.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
