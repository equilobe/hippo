using System.Text.RegularExpressions;
using FluentValidation;
using Hippo.Application.Common.Interfaces;
using Hippo.Application.EnvironmentVariables.Commands;
using Hippo.Core.Enums;
using Hippo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Application.Channels.Commands;

public class PatchChannelCommandValidator : AbstractValidator<PatchChannelCommand>
{
    private readonly Regex validName = new Regex("^[a-zA-Z0-9-_]*$");

    private readonly Regex validDomainName = new Regex(@"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$");

    private readonly Regex validKey = new Regex("^[a-zA-Z0-9-_]*$");

    private readonly IApplicationDbContext _context;

    public PatchChannelCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Name.Value)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(64)
            .Matches(validName)
            .MustAsync(BeUniqueNameForApp).WithMessage("A channel with the same name already exists for this app.")
            .When(v => v.Name.IsSet());

        RuleFor(v => v.Domain.Value)
            .NotEqual("").WithMessage("Domain cannot be an empty string.")
            .Matches(validDomainName)
            .MustAsync(BeUniqueDomainName).WithMessage("The specified domain already exists.")
            .When(v => v.Domain.IsSet());

        RuleFor(v => v.RangeRule)
            .NotEqual("").WithMessage("Range rule cannot be an empty string.");

        RuleFor(v => v.RevisionSelectionStrategy.Value)
            .Must(BeValidRevisionSelectionStrategy).WithMessage("ActiveRevisionId and the specified ChannelRevisionSelectionStrategy do not match up.")
            .When(v => v.RevisionSelectionStrategy.IsSet());

        RuleFor(v => v.EnvironmentVariables.Value)
            .Must(HaveValidKeys).WithMessage("Keys are not valid.")
            .Must(HaveValidValues).WithMessage("Values must not be empty.")
            .When(v => v.EnvironmentVariables.IsSet()); ;

        // TODO: validate RangeRule syntax
    }

    public async Task<bool> BeUniqueNameForApp(PatchChannelCommand command, string name, CancellationToken cancellationToken)
    {
        var appId = _context.Channels
            .First(a => a.Id == command.ChannelId)
            .AppId;

        return await _context.Channels.Where(c => c.AppId == appId).AllAsync(a => a.Name != name, cancellationToken);
    }

    public async Task<bool> BeUniqueDomainName(string domain, CancellationToken cancellationToken)
    {
        return await _context.Channels.AllAsync(a => a.Domain != domain, cancellationToken);
    }

    public bool BeValidRevisionSelectionStrategy(PatchChannelCommand command, ChannelRevisionSelectionStrategy revisionStrategy)
    {
        return (revisionStrategy == ChannelRevisionSelectionStrategy.UseRangeRule && command.ActiveRevisionId is null) ||
            (revisionStrategy == ChannelRevisionSelectionStrategy.UseSpecifiedRevision && command.ActiveRevisionId is not null);
    }

    public bool HaveValidKeys(PatchChannelCommand command, List<UpdateEnvironmentVariableDto>? environmentVariables)
    {
        if (environmentVariables == null)
            return false;

        foreach(var variable in environmentVariables)
        {
            if (variable.Key == string.Empty || !validKey.IsMatch(variable.Key))
                return false;
        }

        return true;
    }

    public bool HaveValidValues(PatchChannelCommand command, List<UpdateEnvironmentVariableDto>? environmentVariables)
    {
        if (environmentVariables == null)
            return false;

        foreach (var variable in environmentVariables)
        {
            if (variable.Value == null)
                return false;
        }

        return true;
    }
}
