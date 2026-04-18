using System.Threading.Tasks;

namespace InventorySystem.Application.Interfaces.Services
{
    public interface IWhatsAppService
    {
        Task<bool> SendInvoiceNotificationAsync(string recipientNumber, string invoiceNumber, decimal amount, string downloadLink);
        Task<bool> SendPaymentReminderAsync(string recipientNumber, string customerName, decimal outstandingAmount);
        Task<bool> SendCustomMessageAsync(string recipientNumber, string message);
    }
}
