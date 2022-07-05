using System.ComponentModel.DataAnnotations;
using Hippo.Application.Common.Exceptions;
using Hippo.Application.Common.Interfaces;
using MediatR;

namespace Hippo.Application.Accounts.Commands;

public class LoginAccountCommand : IRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}

public class LoginAccountCommandHandler : IRequestHandler<LoginAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly ISignInService _signInService;

    public LoginAccountCommandHandler(IIdentityService identityService, ISignInService signInService)
    {
        _identityService = identityService;
        _signInService = signInService;
    }

    public async Task<Unit> Handle(LoginAccountCommand request, CancellationToken cancellationToken)
    {
        var result = await _signInService.PasswordSignInAsync(request.Username, request.Password, request.RememberMe);
        if (!result.Succeeded)
        {
            throw new LoginFailedException(result.Errors);
        }

        return Unit.Value;
    }
}
