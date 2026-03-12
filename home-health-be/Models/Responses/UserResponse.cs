namespace home_health_be.Models.Responses
{
    public record OrganizationUnit(string Id, string Name);

    public record UserResponse(
        string EmployeeId,
        OrganizationUnit BusinessLine,
        OrganizationUnit? Region,
        OrganizationUnit? RegionalDirector,
        OrganizationUnit ExecutiveDirector,
        OrganizationUnit Location
    );

    public record LocationHierarchyRowResponse(
        OrganizationUnit? Region,
        OrganizationUnit? RegionalDirector,
        OrganizationUnit? ExecutiveDirector,
        OrganizationUnit? Location
    );

    public record AuditResponse(
        int PackageID,
        string PackageName,
        int PackageStatus,
        string EDNumber,
        DateTime? StartDate,
        DateTime? EndDate,
        int? FolderID,
        string? PackageScore
    );

    public record CreateAuditResponse(int PackageID);

    public record AuditByIdResponse(
        int PackageID,
        string PackageName,
        int PackageStatus,
        DateTime? StartDate,
        DateTime? EndDate,
        int? FolderID,
        string? PackageScore,
        OrganizationUnit? RegionalDirector,
        OrganizationUnit? ExecutiveDirector
    );

    public record HomeScreenToolsResponse(
        string? TemplateName,
        int TemplateID,
        int? TemplateStatus,
        string? TemplateScore,
        string? AssignedAuditor,
        int? PackageTemplateID,
        string? AuditPlaceLocation,
        string? LocationName,
        bool? AllQuestionsAnswered
    );
}
