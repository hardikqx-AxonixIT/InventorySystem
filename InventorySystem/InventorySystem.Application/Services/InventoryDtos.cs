namespace InventorySystem.Application.Services
{
    public class AdjustmentRequestDto
    {
        public int ProductId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
