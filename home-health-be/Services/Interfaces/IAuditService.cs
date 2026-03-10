using home_health_be.Models.Responses;

namespace home_health_be.Services.Interfaces
{
    public interface IAuditService
    {
        Task<IReadOnlyList<AuditResponse>> GetAuditsAsync();
    }
}
