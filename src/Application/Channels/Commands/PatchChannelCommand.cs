using Hippo.Application.Common.Exceptions;
using Hippo.Application.Common.Interfaces;
using Hippo.Application.EnvironmentVariables.Commands;
using Hippo.Core.Entities;
using Hippo.Core.Enums;
using Hippo.Core.Events;
using Hippo.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Application.Channels.Commands;

public class PatchChannelCommand : IRequest
{
    public Guid ChannelId { get; set; }
    public Field<List<UpdateEnvironmentVariableDto>?> EnvironmentVariables { get; set; } = null!;
    public Field<string> Name { get; set; } = null!;
    public Field<string> Domain { get; set; } = null!;
    public Field<ChannelRevisionSelectionStrategy> RevisionSelectionStrategy { get; set; } = null!;
    public Field<string> RangeRule { get; set; } = null!;
    public Field<Guid?> ActiveRevisionId { get; set; } = null!;
    public Field<Guid?> CertificateId { get; set; } = null!;
}

public class PatchChannelCommandHandler : IRequestHandler<PatchChannelCommand>
{
    private readonly IApplicationDbContext _context;

    public PatchChannelCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(PatchChannelCommand request, CancellationToken cancellationToken)
    {
        var channel = _context.Channels
            .Include(c => c.App)
            .FirstOrDefault(c => c.Id == request.ChannelId);

        _ = channel ?? throw new NotFoundException(nameof(Channel), request.ChannelId);

        request.EnvironmentVariables.WhenSet((envvars) => UpdateEnvironmentVariables(request, channel));
        request.Name.WhenSet((name) => { channel.Name = name; });
        request.Domain.WhenSet((domain) => { channel.Domain = domain; });
        request.RevisionSelectionStrategy.WhenSet((revisionSelectionStrategy) => { channel.RevisionSelectionStrategy = revisionSelectionStrategy; });
        request.RangeRule.WhenSet((rangeRule) => { channel.RangeRule = rangeRule; });
        request.ActiveRevisionId.WhenSet((activeRevisionId) => { channel.ActiveRevisionId = activeRevisionId; });
        request.CertificateId.WhenSet((certificateId) => { channel.CertificateId = certificateId; });

        if (request.RevisionSelectionStrategy.IsSet()
            && request.RevisionSelectionStrategy.Value == ChannelRevisionSelectionStrategy.UseRangeRule
            && request.RangeRule.IsSet()
            && request.RangeRule.Value is null)
        {
            channel.RangeRule = "*";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private void UpdateEnvironmentVariables(PatchChannelCommand request, Channel channel)
    {
        var existingVariables = GetExistingEnvironmentVariables(request.ChannelId);

        var envVariablesToBeAdded = EnvironmentVariablesToBeAdded(existingVariables, request.EnvironmentVariables.Value, channel);
        var envVariablesToBeUpdated = EnvironmentVariablesToBeUpdated(existingVariables, request.EnvironmentVariables.Value);
        var envVariablesToBeDeleted = EnvironmentVariablesToBeRemoved(existingVariables, request.EnvironmentVariables.Value);

        foreach (var entity in envVariablesToBeAdded)
        {
            entity.AddDomainEvent(new CreatedEvent<EnvironmentVariable>(entity));
        }

        _context.EnvironmentVariables.AddRange(envVariablesToBeAdded);

        if (existingVariables.Count > 0 && request.EnvironmentVariables.Value is not null)
        {
            foreach (var environmentVariable in request.EnvironmentVariables.Value)
            {
                var updatedEnvVar = existingVariables.FirstOrDefault(v => v.Key == environmentVariable.Key);

                if (updatedEnvVar is null)
                {
                    continue;
                }
                updatedEnvVar.Key = environmentVariable.Key;
                updatedEnvVar.Value = environmentVariable.Value;

                updatedEnvVar.AddDomainEvent(new ModifiedEvent<EnvironmentVariable>(updatedEnvVar));
            }
        }

        foreach (var entity in envVariablesToBeDeleted)
        {
            entity.AddDomainEvent(new DeletedEvent<EnvironmentVariable>(entity));
        }

        _context.EnvironmentVariables.RemoveRange(envVariablesToBeDeleted);
    }

    private List<EnvironmentVariable> GetExistingEnvironmentVariables(Guid channelId)
    {
        return _context.EnvironmentVariables
            .Where(v => v.ChannelId == channelId)
            .ToList();
    }

    private static List<EnvironmentVariable> EnvironmentVariablesToBeAdded(List<EnvironmentVariable> existingVariables, 
        List<UpdateEnvironmentVariableDto>? environmentVariables, Channel channel)
    {
        if (environmentVariables is null)
        {
            return new List<EnvironmentVariable>();
        }

        var existingVariablesKeys = existingVariables.Select(v => v.Key);

        return environmentVariables
            .Where(v => !existingVariablesKeys.Contains(v.Key))
            .Select(entity => new EnvironmentVariable
            {
                Key = entity.Key,
                Value = entity.Value,
                Channel = channel
            })
            .ToList();
    }

    private static List<EnvironmentVariable> EnvironmentVariablesToBeUpdated(List<EnvironmentVariable> existingVariables,
        List<UpdateEnvironmentVariableDto>? environmentVariables)
    {
        if (environmentVariables is null)
        {
            return new List<EnvironmentVariable>();
        }

        var environmentVariablesKeys = environmentVariables.Select(v => v.Key);

        return existingVariables.Where(v => environmentVariablesKeys.Contains(v.Key)).ToList();
    }

    private static List<EnvironmentVariable> EnvironmentVariablesToBeRemoved(List<EnvironmentVariable> existingVariables,
        List<UpdateEnvironmentVariableDto>? environmentVariables)
    {
        if (environmentVariables is null)
        {
            environmentVariables = new List<UpdateEnvironmentVariableDto>();
        }

        var environmentVariablesKeys = environmentVariables.Select(v => v.Key);

        return existingVariables.Where(v => !environmentVariablesKeys.Contains(v.Key)).ToList();
    }
}
