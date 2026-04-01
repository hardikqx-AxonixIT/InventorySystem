namespace InventorySystem.Domain.Entities
{
    public class JournalVoucherLine : BaseEntity
    {
        public int JournalVoucherId { get; set; }
        public JournalVoucher? JournalVoucher { get; set; }

        public int LedgerId { get; set; }
        public AccountLedger? Ledger { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public string? Remarks { get; set; }
    }
}
