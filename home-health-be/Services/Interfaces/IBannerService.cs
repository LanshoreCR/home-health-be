using home_health_be.Models.Responses;
using System.Security.Claims;

namespace home_health_be.Services.Interfaces
{
    public interface IBannerService
    {
        Task<AuditResponse?> GetBannerByIdAsync(ClaimsPrincipal user, int controller, int packageId);
    }
}
