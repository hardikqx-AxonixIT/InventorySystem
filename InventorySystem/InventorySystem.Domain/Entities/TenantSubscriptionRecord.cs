using System;

namespace InventorySystem.Domain.Entities
{
    public class TenantSubscriptionRecord
    {
        public int Id { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PlanCode { get; set; } = "TRIAL";

        public DateTime StartedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
