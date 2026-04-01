using InventorySystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InventorySystem.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;
        private readonly ITransactionService _transactionService;

        public ExportController(IExportService exportService, ITransactionService transactionService)
        {
            _exportService = exportService;
            _transactionService = transactionService;
        }

        [HttpGet("inventory/excel")]
        public async Task<IActionResult> ExportInventoryExcel()
        {
            var data = await _transactionService.GetReportsSnapshotAsync();
            var fileBytes = await _exportService.GenerateInventoryExcelAsync(data);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Inventory_Report_{System.DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet("sales-invoice/{id:int}/pdf")]
        public async Task<IActionResult> ExportSalesInvoicePdf(int id)
        {
            var fileBytes = await _exportService.GenerateSalesInvoicePdfAsync(id);
            return File(fileBytes, "application/pdf", $"Invoice_{id}.pdf");
        }

        [HttpGet("tally-sales/xml")]
        public async Task<IActionResult> ExportTallySalesXml([FromQuery] System.DateTime? fromDate, [FromQuery] System.DateTime? toDate)
        {
            var from = fromDate ?? System.DateTime.UtcNow.AddMonths(-1);
            var to = toDate ?? System.DateTime.UtcNow;
            var fileBytes = await _exportService.GenerateTallyOrdersXmlAsync(from, to);
            return File(fileBytes, "application/xml", $"Tally_Sales_{from:yyyyMMdd}_{to:yyyyMMdd}.xml");
        }
    }
}
