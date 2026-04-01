using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class PaymentGatewayCallbackLog : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string GatewayName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ExternalOrderId { get; set; }

        [MaxLength(100)]
        public string? ExternalPaymentId { get; set; }

        public bool SignatureVerified { get; set; }
        public string? RawPayload { get; set; }
    }
}
