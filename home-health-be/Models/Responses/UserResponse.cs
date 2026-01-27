namespace home_health_be.Models.Responses
{
    public class UserResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        public bool Exists { get; set; }
    }
}
