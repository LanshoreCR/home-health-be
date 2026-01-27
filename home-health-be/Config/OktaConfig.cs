namespace home_health_be.Config
{
    public class OktaConfig
    {
        public string Domain { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Audience => $"api://default";
    }
}
