using home_health_be.Models.Requests;
using home_health_be.Models.Responses;

namespace home_health_be.Services.Interfaces
{
    public interface IAuditService
    {
        Task<IReadOnlyList<AuditResponse>> GetAuditsAsync();
        Task<CreateAuditResponse> CreateAuditAsync(CreateAuditRequest request);
    }
}
