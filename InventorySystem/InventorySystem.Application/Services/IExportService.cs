using System.IO;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public interface IExportService
    {
        Task<byte[]> GenerateInventoryExcelAsync(object data);
        Task<byte[]> GenerateSalesInvoicePdfAsync(int salesInvoiceId);
        Task<byte[]> GenerateTallyOrdersXmlAsync(DateTime fromDate, DateTime toDate);
    }
}
