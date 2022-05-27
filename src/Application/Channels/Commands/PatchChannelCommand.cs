using AutoMapper;
using Hippo.Application.Common.Config;
using Hippo.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hippo.Application.Channels.Commands;

public class PatchChannelCommand : IRequest<Guid>
{
    public PatchChannelCommand(JsonPatchDocument item, Guid channelId)
    {
        Patch = item;
        ChannelId = channelId;
    }

    [Required]
    public Guid ChannelId { get; set; }

    [Required]
    public JsonPatchDocument Patch { get; set; } = new JsonPatchDocument();
}

public class PatchChannelCommandHandler : IRequestHandler<PatchChannelCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public PatchChannelCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(PatchChannelCommand request, CancellationToken cancellationToken)
    {

        var channel = await _context.Channels
            .Include(c => c.EnvironmentVariables)
            .Include(c => c.App)
            .Where(c => c.Id == request.ChannelId)
            .SingleAsync(cancellationToken);

        request.Patch.ApplyTo(channel);

        await _context.SaveChangesAsync(cancellationToken);

        return channel.Id;
    }
}
