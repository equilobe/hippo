using System.Reflection;
using Hippo.Application.Common.Interfaces;
using Hippo.Core.Entities;
using Hippo.Infrastructure.Data.Interceptors;
using Hippo.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hippo.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<Account>, IApplicationDbContext
{
    private readonly IMediator _mediator;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    private readonly ICurrentUserService _currentUserService;

    private string? UserId => _currentUserService.UserId;
    private bool IsAdministrator => _currentUserService.IsAdministrator;

    public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
            ICurrentUserService currentUserService) : base(options)
    {
        _mediator = mediator;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _currentUserService = currentUserService;
    }

    public ApplicationDbContext(
            IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
            ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _currentUserService = currentUserService;
    }

    public DbSet<App> Apps => Set<App>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<Channel> Channels => Set<Channel>();

    public DbSet<EnvironmentVariable> EnvironmentVariables => Set<EnvironmentVariable>();

    public DbSet<Revision> Revisions => Set<Revision>();

    public DbSet<RevisionComponent> RevisionComponents => Set<RevisionComponent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        builder.Entity<App>().HasQueryFilter(a => a.CreatedBy == UserId || IsAdministrator);

        builder.Entity<Certificate>().HasQueryFilter(c => c.CreatedBy == UserId || IsAdministrator);

        builder.Entity<Channel>().HasQueryFilter(ch => ch.App.CreatedBy == UserId || IsAdministrator);

        builder.Entity<Revision>().HasQueryFilter(r => r.App.CreatedBy == UserId || IsAdministrator);

        builder.Entity<RevisionComponent>().HasQueryFilter(rc => rc.Revision.App.CreatedBy == UserId || IsAdministrator);
        
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        
        await _mediator.DispatchDomainEvents(this);

        return result;
    }
}
