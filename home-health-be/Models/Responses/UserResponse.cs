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
        OrganizationUnit BusinessLine,
        OrganizationUnit? Region,
        OrganizationUnit? RegionalDirector,
        OrganizationUnit? ExecutiveDirector,
        OrganizationUnit? Location
    );
}
