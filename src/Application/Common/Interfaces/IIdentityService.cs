using Hippo.Application.Common.Models;

namespace Hippo.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string> GetUsernameAsync(string userId);

    Task<string> GetUserIdAsync(string username);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<Guid> CreateUserAsync(string username, string? email, string password);
}
