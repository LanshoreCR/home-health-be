using System.Text.Json.Serialization;

namespace home_health_be.Models.Auth
{
    public class OktaUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("preferred_username")]
        public string PreferredName { get; set; } = string.Empty;
    }
}
