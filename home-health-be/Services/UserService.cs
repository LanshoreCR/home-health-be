using home_health_be.Data;
using home_health_be.Models.Data;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace home_health_be.Services
{
    public class UserService(DatabaseContext database, ILogger<UserService> logger) : IUserService
    {
        public async Task<UserResponse?> GetCurrentUserInfoAsync(ClaimsPrincipal user)
        {
            try
            {
                var email = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? "jose.rojasguzman@brightspringhealth.com";

                var emailParam = new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email };

                var row = await database.Database
                    .SqlQueryRaw<UserInfoSpResult>("EXEC [dbo].[USP_HH_UserInfo_Get] @Email", emailParam)
                    .FirstOrDefaultAsync();

                if (row is null)
                    return null;

                return MapToUserResponse(row);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_UserInfo_Get");
                throw;
            }
        }

        private static UserResponse MapToUserResponse(UserInfoSpResult row)
        {
            var businessLine = new OrganizationUnit(row.BusinessLine_ID, row.BusinessLineName);
            var executiveDirector = new OrganizationUnit(row.ED_ID, row.EDName);
            var location = new OrganizationUnit(row.LocationID, row.LocationName);

            OrganizationUnit? region = null;
            if (row.Region_ID is { } regionId && row.RegionName is { } regionName)
                region = new OrganizationUnit(regionId, regionName);

            OrganizationUnit? regionalDirector = null;
            if (row.RD_IDa is { } rdId && row.RDName is { } rdName)
                regionalDirector = new OrganizationUnit(rdId, rdName);

            return new UserResponse(
                row.EmployeeId,
                businessLine,
                region,
                regionalDirector,
                executiveDirector,
                location
            );
        }
    }
}
