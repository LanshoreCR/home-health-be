namespace home_health_be.Models.Data
{
    public class HomeScreenBannerSpResult
    {
        public int PackageID { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public int PackageStatus { get; set; }
        public string EDNumber { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FolderID { get; set; }
        public string? PackageScore { get; set; }
    }
}
