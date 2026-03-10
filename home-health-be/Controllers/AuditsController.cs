using home_health_be.Models.Responses;
using home_health_be.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace home_health_be.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EDExternalPolicy")]
    public class AuditsController(IAuditService auditService, ILogger<AuditsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AuditResponse>>> GetAudits()
        {
            try
            {
                var result = await auditService.GetAuditsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve audits");
                return StatusCode(500, "An error occurred while retrieving audits.");
            }
        }
    }
}
