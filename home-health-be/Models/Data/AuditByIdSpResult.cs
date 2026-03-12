namespace home_health_be.Models.Data
{
    public class AuditByIdSpResult
    {
        public int PackageID { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public int PackageStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FolderID { get; set; }
        public string? PackageScore { get; set; }
        public string? RD_IDa { get; set; }
        public string? RDName { get; set; }
        public string? ED_ID { get; set; }
        public string? EDName { get; set; }
    }
}
