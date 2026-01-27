using home_health_be.Data;
using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using System.Security.Claims;

namespace home_health_be.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext database;

        public UserService(DatabaseContext context)
        {
            database = context;
        }

        Task<UserResponse> IUserService.GetUserInfoAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Task.FromResult(new UserResponse { Exists = false });
            }
            return Task.FromResult(new UserResponse { Id = id, Exists = true });
        }

        public UserResponse GetAuthenticatedUserInfo(ClaimsPrincipal user)
        {
            var id = user.FindFirst("okta_id")?.Value ?? string.Empty;
            var name = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return new UserResponse
            {
                Id = id,
                Name = name,
                Email = email,
                Roles = roles,
                Exists = !string.IsNullOrEmpty(id)
            };
        }
    }
}
