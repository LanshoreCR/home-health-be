using home_health_be.Data;
using home_health_be.Models.Data;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace home_health_be.Services
{
    public class BannerService(DatabaseContext database, ILogger<BannerService> logger) : IBannerService
    {
        private const string HardcodedUserId = "0765647";

        public async Task<AuditResponse?> GetBannerByIdAsync(ClaimsPrincipal user, int controller, int packageId)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("okta_id")?.Value
                    ?? HardcodedUserId;

                var userIdParam = new SqlParameter("@UserID", SqlDbType.NVarChar, 50) { Value = userId };
                var controllerParam = new SqlParameter("@Controller", SqlDbType.Int) { Value = controller };
                var packageIdParam = new SqlParameter("@PackageID", SqlDbType.Int) { Value = packageId };

                var row = await database.Database
                    .SqlQueryRaw<HomeScreenBannerSpResult>(
                        "EXEC [dbo].[USP_HH_HomeScreen_Banner_Search] @UserID, @Controller, @PackageID",
                        userIdParam,
                        controllerParam,
                        packageIdParam)
                    .FirstOrDefaultAsync();

                if (row is null)
                    return null;

                return MapToAuditResponse(row);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_HomeScreen_Banner_Search");
                throw;
            }
        }

        private static AuditResponse MapToAuditResponse(HomeScreenBannerSpResult row) =>
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
