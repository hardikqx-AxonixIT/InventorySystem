using InventorySystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/commercial")]
    [Authorize(Roles = "SuperAdmin")]
    public class CommercialController : ControllerBase
    {
        private readonly ICommercialService _commercial;

        public CommercialController(ICommercialService commercial)
        {
            _commercial = commercial;
        }

        [HttpGet("plans")]
        public IActionResult GetPlans() => Ok(_commercial.GetPlans());

        [HttpPost("trial/start")]
        public IActionResult StartTrial([FromBody] StartTrialRequest request)
        {
            return Ok(_commercial.StartTrial(request.TenantId, request.Email, request.Days <= 0 ? 14 : request.Days));
        }

        [HttpPost("subscription/activate")]
        public IActionResult Activate([FromBody] ActivatePlanRequest request)
        {
            try
            {
                return Ok(_commercial.ActivatePlan(request.TenantId, request.PlanCode, request.Months <= 0 ? 1 : request.Months));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("license/generate")]
        public IActionResult GenerateLicense([FromBody] GenerateLicenseRequest request)
        {
            return Ok(_commercial.GenerateLicense(request.TenantId, request.Months <= 0 ? 1 : request.Months));
        }

        [AllowAnonymous]
        [HttpPost("license/validate")]
        public IActionResult ValidateLicense([FromBody] ValidateLicenseRequest request)
        {
            return Ok(_commercial.ValidateLicense(request.LicenseKey));
        }

        [HttpGet("subscription/{tenantId}")]
        public IActionResult GetSubscription(string tenantId)
        {
            return Ok(_commercial.GetSubscription(tenantId));
        }
    }

    public class StartTrialRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Days { get; set; } = 14;
    }

    public class ActivatePlanRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public string PlanCode { get; set; } = "STARTER";
        public int Months { get; set; } = 1;
    }

    public class GenerateLicenseRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public int Months { get; set; } = 1;
    }

    public class ValidateLicenseRequest
    {
        public string LicenseKey { get; set; } = string.Empty;
    }
}
