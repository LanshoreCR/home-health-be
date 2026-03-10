using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_health_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EDExternalPolicy")]
    public class BannerController(IBannerService bannerService, ILogger<BannerController> logger) : ControllerBase
    {
        [HttpGet("home-screen")]
        public async Task<ActionResult<HomeScreenBannerResponse>> GetHomeScreenBanner([FromQuery] int controller = 1)
        {
            try
            {
                var result = await bannerService.GetHomeScreenBannerAsync(User, controller);
                return result is null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve home screen banner");
                return StatusCode(500, "An error occurred while retrieving the home screen banner.");
            }
        }
    }
}
