using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class JournalVoucher : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Narration { get; set; }

        [MaxLength(50)]
        public string? SourceModule { get; set; }

        [MaxLength(50)]
        public string? SourceDocumentNo { get; set; }

        public ICollection<JournalVoucherLine> Lines { get; set; } = new List<JournalVoucherLine>();
    }
}
