namespace home_health_be.Models.Responses
{
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
}
