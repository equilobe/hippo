using System.Security.Claims;
using Hippo.Application.Common.Interfaces;
using Hippo.Application.Common.Security;

namespace Hippo.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAdministrator => _httpContextAccessor.HttpContext?.User?.IsInRole(UserRole.Administrator) ?? false;
}
