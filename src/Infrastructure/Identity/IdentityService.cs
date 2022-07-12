using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Models;
using Hippo.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<Account> _userManager;

    private readonly IUserClaimsPrincipalFactory<Account> _userClaimsPrincipalFactory;

    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
            UserManager<Account> userManager,
            IUserClaimsPrincipalFactory<Account> userClaimsPrincipalFactory,
            IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
        return user.UserName;
    }

    public async Task<string> GetUserIdAsync(string userName)
    {
        var user = await _userManager.Users.FirstAsync(u => u.UserName == userName);
        return user.Id;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new Account
        {
            UserName = userName,
            Email = userName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded && _userManager.Users.Count() == 1)
        {
            await _userManager.AddToRoleAsync(user, UserRole.Administrator);
        }

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);

        return user is not null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
        if (user is null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);

        return user is not null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(Account user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<bool> CheckPasswordAsync(string userName, string password)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.UserName == userName);

        if (user is null)
        {
            return false;
        }

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<string[]> GetUserNamesAsync()
    {
        return await _userManager.Users.Select(a => a.UserName).ToArrayAsync();
    }
    
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = _userManager.Users.Single(u => u.Id == userId);

        return await _userManager.GetRolesAsync(user);
    }
}
