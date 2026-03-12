using home_health_be.Models.Requests;
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

        [HttpGet("{packageId}/tools")]
        public async Task<ActionResult<IReadOnlyList<HomeScreenToolsResponse>>> GetToolsByPackageId(int packageId)
        {
            try
            {
                var result = await auditService.GetToolsByPackageIdAsync(packageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve tools for package");
                return StatusCode(500, "An error occurred while retrieving tools.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditByIdResponse>> GetAuditById(int id, [FromQuery] int controller = 1)
        {
            try
            {
                var result = await auditService.GetAuditByIdAsync(User, controller, id);
                return result is null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve audit by id");
                return StatusCode(500, "An error occurred while retrieving the audit.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreateAuditResponse>> CreateAudit([FromBody] CreateAuditRequest request)
        {
            if (request is null)
                return BadRequest("Request body is required.");

            if (request.EndDate < request.StartDate)
                return BadRequest("EndDate must be on or after StartDate.");

            try
            {
                var result = await auditService.CreateAuditAsync(request);
                return CreatedAtAction(nameof(GetAuditById), new { id = result.PackageID }, result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create audit");
                return StatusCode(500, "An error occurred while creating the audit.");
            }
        }
    }
}
