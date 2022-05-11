using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Models;
using Hippo.Application.Revisions.Commands;
using Hippo.Core.Entities;
using Hippo.Core.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hippo.Application.Apps.EventHandlers;

public class InitialRevisionImport : INotificationHandler<DomainEventNotification<CreatedEvent<App>>>
{
    private readonly IBindleService _bindleService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<InitialRevisionImport> _logger;
    private readonly IMediator _mediator;

    public InitialRevisionImport(IBindleService bindleService,
        IApplicationDbContext context,
        ILogger<InitialRevisionImport> logger,
        IMediator mediator)
    {
        _bindleService = bindleService;
        _context = context;
        _logger = logger;        
        _mediator = mediator;
    }

    public async Task Handle(DomainEventNotification<CreatedEvent<App>> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var app = domainEvent.Entity;

        var allAppRevisions = await _bindleService.GetBindleRevisionNumbers(app.StorageId);
        var existingRevisions = _context.Revisions.Where(r => r.AppId == app.Id).ToList();
        var missingRevisions = GetMissingRevisions(allAppRevisions, existingRevisions);

        foreach (var revision in missingRevisions)
        {
            var command = new CreateRevisionCommand
            {
                AppId = app.Id,
                RevisionNumber = revision,
            };

            await _mediator.Send(command, cancellationToken);
        }

        _logger.LogInformation("Hippo Domain Event: {DomainEvent}", domainEvent.GetType().Name);
    }

    private static IEnumerable<string> GetMissingRevisions(IEnumerable<string?> allAppRevisions, List<Revision> existingRevisions)
    {
        return allAppRevisions
            .Where(revision => !existingRevisions.Any(er => er.RevisionNumber == revision))
            .Where(revision => revision is not null)
            .Select(revision => new string(revision))
            .ToList();
    }
}
