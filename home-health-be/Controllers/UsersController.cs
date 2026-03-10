using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_health_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EDExternalPolicy")]
    public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            try
            {
                var result = await userService.GetCurrentUserInfoAsync(User);
                return result is null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve current user info");
                return StatusCode(500, "An error occurred while retrieving user information.");
            }
        }
    }
}
