using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_health_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EDExternalPolicy")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public ActionResult<UserResponse> GetCurrentUser()
        {
            var result = _userService.GetAuthenticatedUserInfo(User);
            return result.Exists ? Ok(result) : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(string id)
        {
            var result = await _userService.GetUserInfoAsync(id);
            return result.Exists ? Ok(result) : NotFound();
        }
    }
}
