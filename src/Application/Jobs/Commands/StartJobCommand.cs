using Hippo.Application.Common.Exceptions;
using Hippo.Application.Common.Interfaces;
using Hippo.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hippo.Application.Jobs.Commands;

public class StartJobCommand : IRequest
{
    public string JobName { get; set; } = null!;
}

public class StartJobCommandHandler : IRequestHandler<StartJobCommand>
{
    private readonly IJobService _jobService;
    private readonly ILogger<StartJobCommandHandler> _logger;
    private readonly IApplicationDbContext _context;

    public StartJobCommandHandler(
        IJobService jobService,
        ILogger<StartJobCommandHandler> logger,
        IApplicationDbContext context)
    {
        _jobService = jobService;
        _logger = logger;
        _context = context;
    }

    public async Task<Unit> Handle(StartJobCommand request, CancellationToken cancellationToken)
    {
        var channel = await _context.Channels
            .Include(c => c.App)
            .Include(c => c.EnvironmentVariables)
            .Include(c => c.ActiveRevision)
            .FirstOrDefaultAsync(c => c.Id.ToString() == request.JobName);
        if(channel is null)
        {
            throw new NotFoundException(nameof(Channel), request.JobName);
        }

        if (channel.ActiveRevision is not null)
        {
            _logger.LogInformation($"{channel.App.Name}: Starting channel {channel.Name} at revision {channel.ActiveRevision.RevisionNumber}");
            var envvars = channel.EnvironmentVariables.ToDictionary(
                e => e.Key!,
                e => e.Value!
            );
            _jobService.StartJob(channel.Id, $"{channel.App.StorageId}/{channel.ActiveRevision.RevisionNumber}", envvars, channel.Domain);
            _logger.LogInformation($"Started {channel.App.Name} Channel {channel.Name} at revision {channel.ActiveRevision.RevisionNumber}");
        }
        else
        {
            _logger.LogInformation($"Not starting {channel.App.Name} Channel {channel.Name}: no active revision");
        }

        return Unit.Value;
    }
}
