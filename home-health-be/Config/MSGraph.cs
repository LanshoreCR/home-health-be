using home_health_be.Models.Auth;
using Microsoft.Graph;
using System.Globalization;
using User = Microsoft.Graph.Models.User;

namespace home_health_be.Config
{
    public sealed class MSGraph
    {
        private readonly GraphServiceClient _graphClient;
        private readonly IConfiguration _configuration;

        public MSGraph(
            GraphServiceClient graphClient,
            IConfiguration configuration)
        {
            _graphClient = graphClient;
            _configuration = configuration;
        }

        public async Task<User?> GetUserByCredentialsAsync(
            string email,
            string name,
            string oktaCredential)
        {
            var formattedName = CultureInfo
                .CurrentCulture
                .TextInfo
                .ToTitleCase(name.ToLowerInvariant());

            var filter =
                $"mail eq '{email}' or " +
                $"userPrincipalName eq '{email}' or " +
                $"displayName eq '{formattedName}' or " +
                $"mail eq '{oktaCredential}' or " +
                $"userPrincipalName eq '{oktaCredential}'";

            var users = await _graphClient.Users.GetAsync(config =>
            {
                config.QueryParameters.Filter = filter;
            });

            return users?.Value?.FirstOrDefault();
        }

        public async Task<List<string>> GetUserRolesAsync(OktaUser userInfo)
        {
            var rolesConfig =
                _configuration.GetSection("ADSecurityGroups").Get<ADGroups>();

            var user = await GetUserByCredentialsAsync(
                userInfo.Email,
                userInfo.Name,
                userInfo.PreferredName);

            if (user == null)
            {
                return new List<string> { "No Access" };
            }

            var groups = await _graphClient
                .Users[user.Id]
                .MemberOf
                .GetAsync();

            if (groups?.Value == null)
            {
                return new List<string> { "No Access" };
            }

            var userRoles = new List<string>();

            if (groups.Value.Any(g => g.Id == rolesConfig?.User))
            {
                userRoles.Add("User");
            }

            if (groups.Value.Any(g => g.Id == rolesConfig?.Approver))
            {
                userRoles.Add("Approver");
            }

            if (groups.Value.Any(g => g.Id == rolesConfig?.Report))
            {
                userRoles.Add("Report");
            }

            if (groups.Value.Any(g => g.Id == rolesConfig?.Leader))
            {
                userRoles.Add("Leader");
            }

            if (!userRoles.Any())
            {
                userRoles.Add("No Access");
            }

            return userRoles;
        }

        public async Task<List<User>> GetUsersByGroupAsync()
        {
            var groupId = _configuration
                .GetSection("AzureCapaSecurityGroups")
                .GetSection("User")
                .Value;

            var members = await _graphClient
                .Groups[groupId]
                .Members
                .GetAsync(config =>
                {
                    config.QueryParameters.Top = 999;
                });

            return members?.Value?
                .OfType<User>()
                .ToList()
                ?? new List<User>();
        }
    }

    public sealed class GraphClientConfig
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string Scopes { get; set; } = string.Empty;
    }

    public sealed class ADGroups
    {
        public string User { get; set; } = string.Empty;
        public string Approver { get; set; } = string.Empty;
        public string Report { get; set; } = string.Empty;
        public string Leader { get; set; } = string.Empty;
    }
}
