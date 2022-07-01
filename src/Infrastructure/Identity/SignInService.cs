using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Models;
using MediatR;

namespace Hippo.Infrastructure.Identity;

public class SignInService : ISignInService
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly PasswordHasher _passwordHasher;

    public SignInService(IApplicationDbContext applicationDbContext, PasswordHasher passwordHasher)
    {
        _applicationDbContext = applicationDbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> PasswordSignInAsync(string username, string password, bool rememberMe = false)
    {
        var user = _applicationDbContext.Users.SingleOrDefault(u => u.Username == username);

        if (user is null)
        {
            return Result.Failure(new string[] { "Account does not exist." });
        }

        return _passwordHasher.VerifyHashedPassword(user.Password, password);
    }

    public async Task<Unit> SignOutAsync()
    {
        return Unit.Value;
    }
}
