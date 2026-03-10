using home_health_be.Data;
using home_health_be.Models.Data;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace home_health_be.Services
{
    public class LocationService(DatabaseContext database, ILogger<LocationService> logger) : ILocationService
    {
        public async Task<IReadOnlyList<LocationHierarchyRowResponse>> GetLocationHierarchyAsync(string? rdId, string? edId)
        {
            try
            {
                var rdParam = new SqlParameter("@V_RD", SqlDbType.NVarChar, 50) { Value = (object?)rdId ?? DBNull.Value };
                var edParam = new SqlParameter("@V_ED", SqlDbType.NVarChar, 50) { Value = (object?)edId ?? DBNull.Value };

                var rows = await database.Database
                    .SqlQueryRaw<LocationFilteringSpResult>(
                        "EXEC [dbo].[USP_HH_LocationFiltering] @V_RD, @V_ED",
                        rdParam,
                        edParam)
                    .ToListAsync();

                return rows.Select(MapToLocationHierarchyRow).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing USP_HH_LocationFiltering");
                throw;
            }
        }

        private static LocationHierarchyRowResponse MapToLocationHierarchyRow(LocationFilteringSpResult row)
        {
            var businessLine = new OrganizationUnit(row.BusinessLine_ID, row.BusinessLine);

            OrganizationUnit? region = null;
            if (row.Region_ID is { } regionId && row.RegionName is { } regionName)
                region = new OrganizationUnit(regionId, regionName);

            OrganizationUnit? regionalDirector = null;
            if (row.RD_IDa is { } rdId && row.RDName is { } rdName)
                regionalDirector = new OrganizationUnit(rdId, rdName);

            OrganizationUnit? executiveDirector = null;
            if (row.ED_ID is { } edId && row.EDName is { } edName)
                executiveDirector = new OrganizationUnit(edId, edName);

            OrganizationUnit? location = null;
            if (row.LocationID is { } locationId && row.LocationName is { } locationName)
                location = new OrganizationUnit(locationId, locationName);

            return new LocationHierarchyRowResponse(
                businessLine,
                region,
                regionalDirector,
                executiveDirector,
                location);
        }
    }
}
