using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Security;
using Hippo.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IApplicationDbContext _applicationDbContext;

    public IdentityService(
            IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;   
    }

    public async Task<string> GetUsernameAsync(string userId)
    {
        var user = await _applicationDbContext.Users.FirstAsync(u => u.Id == Guid.Parse(userId));
        return user.Username;
    }

    public async Task<string> GetUserIdAsync(string username)
    {
        var user = await _applicationDbContext.Users.FirstAsync(u => u.Username == username);
        return user.Id.ToString();
    }

    public async Task<Guid> CreateUserAsync(string username, string? email, string password)
    {
        var role = _applicationDbContext.Users.Any() ? UserRole.Standard : UserRole.Administrator;

        var user = new User
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
        };

        await _applicationDbContext.Users.AddAsync(user);

        await _applicationDbContext.SaveChangesAsync(new CancellationToken()); 

        return user.Id;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _applicationDbContext.Users.SingleOrDefaultAsync(u => u.Id == Guid.Parse(userId));

        return user is not null && user.Role == role;
    }
}
