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
}
