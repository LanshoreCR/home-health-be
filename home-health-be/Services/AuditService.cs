using home_health_be.Data;
using home_health_be.Models.Data;
using home_health_be.Models.Requests;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

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

        public async Task<AuditResponse?> GetAuditByIdAsync(ClaimsPrincipal user, int controller, int packageId)
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

        public async Task<IReadOnlyList<HomeScreenToolsResponse>> GetToolsByPackageIdAsync(int packageId)
        {
            try
            {
                var packageIdParam = new SqlParameter("@PackageID", SqlDbType.Int) { Value = packageId };

                var rows = await database.Database
                    .SqlQueryRaw<HomeScreenToolsSpResult>(
                        "EXEC [dbo].[USP_HH_HomeScreen_Tools] @PackageID",
                        packageIdParam)
                    .ToListAsync();

                return rows.Select(MapToHomeScreenToolsResponse).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_HomeScreen_Tools");
                throw;
            }
        }

        public async Task<CreateAuditResponse> CreateAuditAsync(CreateAuditRequest request)
        {
            try
            {
                var edIdParam = new SqlParameter("@EDId", SqlDbType.VarChar, 10) { Value = request.EDId };
                var createdByParam = new SqlParameter("@CreatedBy", SqlDbType.VarChar, 10) { Value = HardcodedUserId };
                var startDateParam = new SqlParameter("@StartDate", SqlDbType.Date) { Value = request.StartDate };
                var endDateParam = new SqlParameter("@EndDate", SqlDbType.Date) { Value = request.EndDate };
                var packageIdParam = new SqlParameter("@PackageID", SqlDbType.Int) { Direction = ParameterDirection.Output };

                await database.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[USP_HH_Package_Create] @EDId, @CreatedBy, @StartDate, @EndDate, @PackageID OUTPUT",
                    edIdParam,
                    createdByParam,
                    startDateParam,
                    endDateParam,
                    packageIdParam);

                var packageId = packageIdParam.Value is int id ? id : 0;
                return new CreateAuditResponse(packageId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_Package_Create");
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

        private static HomeScreenToolsResponse MapToHomeScreenToolsResponse(HomeScreenToolsSpResult row) =>
            new(
                row.TemplateName,
                row.TemplateID,
                row.TemplateStatus,
                row.TemplateScore,
                row.AssignedAuditor,
                row.PackageTemplateID,
                row.AuditPlaceLocation,
                row.LocationName,
                row.AllQuestionsAnswered);
    }
}
