using home_health_be.Models.Requests;
using home_health_be.Models.Responses;
using System.Security.Claims;

namespace home_health_be.Services.Interfaces
{
    public interface IAuditService
    {
        Task<IReadOnlyList<AuditResponse>> GetAuditsAsync();
        Task<AuditByIdResponse?> GetAuditByIdAsync(ClaimsPrincipal user, int controller, int packageId);
        Task<IReadOnlyList<HomeScreenToolsResponse>> GetToolsByPackageIdAsync(int packageId);
        Task<CreateAuditResponse> CreateAuditAsync(CreateAuditRequest request);
        Task<CreateToolResponse> CreateToolAsync(CreateToolRequest request);
    }
}
