using home_health_be.Models.Responses;

namespace home_health_be.Services.Interfaces
{
    public interface ILocationService
    {
        Task<IReadOnlyList<LocationHierarchyRowResponse>> GetLocationHierarchyAsync(string? rdId, string? edId);
    }
}
