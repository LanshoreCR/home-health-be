namespace home_health_be.Models.Data
{
    /// <summary>
    /// Property names must match the result set columns of [dbo].[USP_HH_HomeScreen_Banner].
    /// Update this class when the SP result shape is known.
    /// </summary>
    public class HomeScreenBannerSpResult
    {
        public string? Message { get; set; }
    }
}
