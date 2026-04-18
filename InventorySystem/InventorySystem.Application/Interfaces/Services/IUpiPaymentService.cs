namespace InventorySystem.Application.Interfaces.Services
{
    public interface IUpiPaymentService
    {
        /// <summary>
        /// Generates a standard upi://pay URI string.
        /// </summary>
        string GenerateUpiUri(string vpa, string payeeName, string transactionNote, string transactionReference, decimal amount);

        /// <summary>
        /// Generates a Base64 encoded QR code PNG image string for the given UPI URI.
        /// </summary>
        string GenerateUpiQrCodeBase64(string upiUri);
    }
}
