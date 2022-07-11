using Hippo.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hippo.Application.Jobs.Commands;

public class StopJobCommand : IRequest
{
    public string JobName { get; set; } = null!;
}

public class StopJobCommandHandler : IRequestHandler<StopJobCommand>
{
    private readonly IJobService _jobService;
    private readonly ILogger<StopJobCommandHandler> _logger;

    public StopJobCommandHandler(IJobService jobService, ILogger<StopJobCommandHandler> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    public Task<Unit> Handle(StopJobCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _jobService.DeleteJob(request.JobName);
        }
        catch(Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
        return Task.FromResult(Unit.Value);
    }
}