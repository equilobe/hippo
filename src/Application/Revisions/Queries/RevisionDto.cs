using Hippo.Application.Common.Mappings;
using Hippo.Core.Entities;

namespace Hippo.Application.Revisions.Queries;

public class RevisionDto : IMapFrom<Revision>
{
    public Guid Id { get; set; }

    public Guid AppId { get; set; }

    public string? RevisionNumber { get; set; }

    public string OrderKey()
    {
        if (SemVer.Version.TryParse(RevisionNumber, out var version))
        {
            return $"{version.Major:D9}{version.Minor:D9}{version.Patch:D9}{RevisionNumber}";
        }
        return RevisionNumber!;
    }
}