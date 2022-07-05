using System.Text.RegularExpressions;
using FluentValidation;
using Hippo.Application.Common.Interfaces;

namespace Hippo.Application.Accounts.Commands;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    private readonly IIdentityService _identityService;

    private readonly Regex validUsername = new Regex("^[a-zA-Z0-9-_]*$");

    public CreateAccountCommandValidator(IIdentityService identityService)
    {
        _identityService = identityService;

        RuleFor(a => a.Username)
            .NotEmpty().WithMessage("The username cannot be empty")
            .MaximumLength(64).WithMessage("The username cannot be longer than 64 characters")
            .Matches(validUsername).WithMessage("The username cannot contain special characters")
            .MustAsync(BeUniqueUsername).WithMessage("The specified username already exists.");

        RuleFor(a => a.Password)
            .NotEmpty().WithMessage("The password cannot be empty")
            .MinimumLength(6).WithMessage("The password must be at least 6 characters long");
    }

    public async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
        try
        {
            await _identityService.GetUserIdAsync(username);
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }
}
