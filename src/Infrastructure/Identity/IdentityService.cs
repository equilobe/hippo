using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Models;
using Hippo.Application.Common.Security;
using Hippo.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

namespace Hippo.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IUserClaimsPrincipalFactory<Account> _userClaimsPrincipalFactory;

    private readonly IAuthorizationService _authorizationService;

    private readonly IApplicationDbContext _applicationDbContext;

    public IdentityService(
            IApplicationDbContext applicationDbContext,
            IUserClaimsPrincipalFactory<Account> userClaimsPrincipalFactory,
            IAuthorizationService authorizationService)
    {
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<string> GetUserNameAsync(string userId)
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
        var role = (_applicationDbContext.Users.Any()) ? UserRole.Administrator : UserRole.Standard;

        var user = new User
        {
            Username = username,
            Email = email,
            Password = password,
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
