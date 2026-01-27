using home_health_be.Config;
using home_health_be.Models.Auth;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http.Headers;
using System.Text.Json;

namespace home_health_be.Services.Auth
{
    public class OktaService : IOktaService
    {
        private readonly OktaConfig? config;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;

        public OktaService(OktaConfig? config)
        {
            this.config = config;

            var metadataAddress = $"{this.config?.Domain}/.well-known/openid-configuration?client_id={this.config?.ClientId}";
            this.configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
        }

        async Task<OktaUser?> IOktaService.GetUserInfoAsync(string endpoint, string accessToken)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<OktaUser>(content);
                return userInfo;
            }
            return null;
        }

        Task<string> IOktaService.GetIntrospectionEndpointAsync()
        {
            throw new NotImplementedException();
        }

        async Task<OpenIdConnectConfiguration?> IOktaService.GetOpenIdConnectConfigurationAsync()
        {
            var discoveryDocument = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            return discoveryDocument;
        }

        public Task<string> GetUserInfoEndpointAsync()
        {
            throw new NotImplementedException();
        }
    }
}
