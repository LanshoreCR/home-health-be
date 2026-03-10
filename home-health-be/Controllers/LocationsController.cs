using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_health_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EDExternalPolicy")]
    public class LocationsController(ILocationService locationService, ILogger<LocationsController> logger) : ControllerBase
    {
        [HttpGet("hierarchy")]
        public async Task<ActionResult<IReadOnlyList<LocationHierarchyRowResponse>>> GetLocationHierarchy(
            [FromQuery] string? rdId,
            [FromQuery] string? edId)
        {
            try
            {
                var result = await locationService.GetLocationHierarchyAsync(rdId, edId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve location hierarchy");
                return StatusCode(500, "An error occurred while retrieving location hierarchy.");
            }
        }
    }
}
