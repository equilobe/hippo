using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Models;
using MediatR;

namespace Hippo.Infrastructure.Identity;

public class SignInService : ISignInService
{
    private readonly IApplicationDbContext _applicationDbContext;

    public SignInService(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Task<Result> PasswordSignInAsync(string username, string password, bool rememberMe = false)
    {
        var user = _applicationDbContext.Users.SingleOrDefault(u => u.Username == username);

        if (user is null)
        {
            return Task.FromResult(Result.Failure(new string[] { "Account does not exist." }));
        }

        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.Password) ? Result.Success() : Result.Failure(new string[] { "Incorrect password." }));
    }

    public Task<Unit> SignOutAsync()
    {
        return Task.FromResult(Unit.Value);
    }
}
