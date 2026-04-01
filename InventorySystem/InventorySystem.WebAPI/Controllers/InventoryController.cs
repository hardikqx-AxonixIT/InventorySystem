using InventorySystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InventorySystem.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IAdjustmentService _adjustmentService;
        private readonly IStockService _stockService;

        public InventoryController(IAdjustmentService adjustmentService, IStockService stockService)
        {
            _adjustmentService = adjustmentService;
            _stockService = stockService;
        }

        [HttpPost("adjustments/request")]
        public async Task<IActionResult> RequestAdjustment([FromBody] AdjustmentRequestDto request)
        {
            var username = User.Identity?.Name ?? "Unknown";
            var result = await _adjustmentService.RequestAdjustmentAsync(request.ProductId, request.Amount, request.Reason, username);
            return Ok(result);
        }

        [Authorize(Policy = "CanApproveAdjustments")]
        [HttpPost("adjustments/{id}/approve")]
        public async Task<IActionResult> ApproveAdjustment(int id)
        {
            var username = User.Identity?.Name ?? "Unknown";
            var success = await _adjustmentService.ApproveAdjustmentAsync(id, username);
            if (success) return Ok();
            return BadRequest("Approval failed or request not found.");
        }

        [Authorize(Policy = "CanApproveAdjustments")]
        [HttpGet("adjustments/pending")]
        public async Task<IActionResult> GetPendingAdjustments()
        {
            var requests = await _adjustmentService.GetPendingRequestsAsync();
            return Ok(requests);
        }
    }
}
