namespace home_health_be.Models.Data
{
    public class HomeScreenToolsSpResult
    {
        public string? TemplateName { get; set; }
        public int TemplateID { get; set; }
        public int? TemplateStatus { get; set; }
        public string? TemplateScore { get; set; }
        public string? AssignedAuditor { get; set; }
        public int? PackageTemplateID { get; set; }
        public string? AuditPlaceLocation { get; set; }
        public string? LocationName { get; set; }
        public bool? AllQuestionsAnswered { get; set; }
    }
}
