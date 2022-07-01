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

    public async Task<Result> PasswordSignInAsync(string username, string password, bool rememberMe = false)
    {
        var user = _applicationDbContext.Users.SingleOrDefault(u => u.Username == username);

        if (user is null)
        {
            return Result.Failure(new string[] { "Account does not exist." });
        }

        return user.Password == password ? Result.Success() : Result.Failure(new string[] { "Password is not valid." });
    }

    public async Task<Unit> SignOutAsync()
    {
        return Unit.Value;
    }
}
