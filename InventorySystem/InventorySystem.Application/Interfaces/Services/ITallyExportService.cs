using System;
using System.Threading.Tasks;

namespace InventorySystem.Application.Interfaces.Services
{
    public interface ITallyExportService
    {
        /// <summary>
        /// Exports Sales Invoices within a date range to Tally XML format.
        /// </summary>
        Task<string> ExportSalesInvoicesToTallyXmlAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Exports Account Ledgers/Customers to Tally XML format.
        /// </summary>
        Task<string> ExportLedgersToTallyXmlAsync();
    }
}
