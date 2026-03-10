namespace home_health_be.Models.Data
{
    public class LocationFilteringSpResult
    {
        public string BusinessLine_ID { get; set; } = string.Empty;
        public string BusinessLine { get; set; } = string.Empty;
        public string? Region_ID { get; set; }
        public string? RegionName { get; set; }
        public string? RD_IDa { get; set; }
        public string? RDName { get; set; }
        public string? ED_ID { get; set; }
        public string? EDName { get; set; }
        public string? LocationID { get; set; }
        public string? LocationName { get; set; }
    }
}
