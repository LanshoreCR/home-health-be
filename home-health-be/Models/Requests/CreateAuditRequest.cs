using System.ComponentModel.DataAnnotations;

namespace home_health_be.Models.Requests
{
    public record CreateAuditRequest(
        [Required]
        [StringLength(10)]
        string EDId,

        [Required]
        DateTime StartDate,

        [Required]
        DateTime EndDate
    );
}
