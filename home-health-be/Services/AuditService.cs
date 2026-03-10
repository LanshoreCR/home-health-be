using home_health_be.Data;
using home_health_be.Models.Data;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace home_health_be.Services
{
    public class AuditService(DatabaseContext database, ILogger<AuditService> logger) : IAuditService
    {
        private const string HardcodedUserId = "0765647";

        public async Task<IReadOnlyList<AuditResponse>> GetAuditsAsync()
        {
            try
            {
                var userParam = new SqlParameter("@UserID", SqlDbType.NVarChar, 50) { Value = HardcodedUserId };
                var controllerParam = new SqlParameter("@Controller", SqlDbType.Int) { Value = 1 };

                var rows = await database.Database
                    .SqlQueryRaw<HomeScreenBannerSpResult>(
                        "EXEC [dbo].[USP_HH_HomeScreen_Banner] @UserID, @Controller",
                        userParam,
                        controllerParam)
                    .ToListAsync();

                return rows.Select(MapToAuditResponse).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_HomeScreen_Banner");
                throw;
            }
        }

        private static AuditResponse MapToAuditResponse(HomeScreenBannerSpResult row)
        {
            return new AuditResponse(
                row.PackageID,
                row.PackageName,
                row.PackageStatus,
                row.EDNumber,
                row.StartDate,
                row.EndDate,
                row.FolderID,
                row.PackageScore
            );
        }
    }
}
