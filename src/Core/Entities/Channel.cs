namespace Hippo.Core.Entities;

public class Channel : AuditableEntity, IHasDomainEvent
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Domain { get; set; }

    public ChannelRevisionSelectionStrategy RevisionSelectionStrategy { get; set; }

    public string? RangeRule { get; set; }

    public Guid? ActiveRevisionId { get; set; }

    public Revision? ActiveRevision { get; set; }

    public Guid? CertificateId { get; set; }

    public Certificate? Certificate { get; set; }

    public int PortId { get; set; }

    public Guid AppId { get; set; }

    public App App { get; set; } = null!;

    public IList<EnvironmentVariable> EnvironmentVariables { get; set; } = new List<EnvironmentVariable>();

    public List<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();
}
