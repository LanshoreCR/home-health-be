using System.ComponentModel.DataAnnotations;

namespace home_health_be.Models.Requests
{
    public record CreateToolRequest(
        [Required]
        [StringLength(5)]
        string EDNumber,

        [Required]
        DateTime StartDate,

        [Required]
        DateTime EndDate
    );
}
