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

        public async Task<HomeScreenBannerResponse?> GetHomeScreenBannerAsync(ClaimsPrincipal user, int controller)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("okta_id")?.Value
                    ?? HardcodedUserId;

                var userIdParam = new SqlParameter("@UserID", SqlDbType.NVarChar, 50) { Value = userId };
                var controllerParam = new SqlParameter("@Controller", SqlDbType.Int) { Value = controller };

                var row = await database.Database
                    .SqlQueryRaw<HomeScreenBannerSpResult>(
                        "EXEC [dbo].[USP_HH_HomeScreen_Banner] @UserID, @Controller",
                        userIdParam,
                        controllerParam)
                    .FirstOrDefaultAsync();

                if (row is null)
                    return null;

                return new HomeScreenBannerResponse(row.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_HomeScreen_Banner");
                throw;
            }
        }
    }
}
