using home_health_be.Models.Auth;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace home_health_be.Services.Auth
{
    public interface IOktaService
    {
        Task<OktaUser?> GetUserInfoAsync(string endpoint, string accessToken);
        Task<OpenIdConnectConfiguration?> GetOpenIdConnectConfigurationAsync();
    }
}
