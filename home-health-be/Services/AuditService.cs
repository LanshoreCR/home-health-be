using home_health_be.Data;
using home_health_be.Models.Data;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace home_health_be.Services
{
    public class AuditService(DatabaseContext database, ILogger<AuditService> logger) : IAuditService
    {
        public async Task<IReadOnlyList<AuditResponse>> GetAuditsAsync()
        {
            try
            {
                var rows = await database.Database
                    .SqlQueryRaw<AuditSpResult>("EXEC [dbo].[USP_HH_Audits_Get]")
                    .ToListAsync();

                return rows.Select(MapToAuditResponse).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_Audits_Get");
                throw;
            }
        }

        private static AuditResponse MapToAuditResponse(AuditSpResult row) =>
            new(
                row.PackageID,
                row.PackageName,
                row.PackageStatus,
                row.EDNumber,
                row.StartDate,
                row.EndDate,
                row.FolderID,
                row.PackageScore);
    }
}
