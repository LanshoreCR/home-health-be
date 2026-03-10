namespace home_health_be.Models.Data
{
    public class UserInfoSpResult
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string BusinessLine_ID { get; set; } = string.Empty;
        public string BusinessLineName { get; set; } = string.Empty;
        public string? Region_ID { get; set; }
        public string? RegionName { get; set; }
        public string? RD_IDa { get; set; }
        public string? RDName { get; set; }
        public string ED_ID { get; set; } = string.Empty;
        public string EDName { get; set; } = string.Empty;
        public string LocationID { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
    }
}
