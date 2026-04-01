using System;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string EntityName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string EntityId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        
        public string? OldValues { get; set; } // JSON
        
        public string? NewValues { get; set; } // JSON
        
        [MaxLength(100)]
        public string? UserId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
