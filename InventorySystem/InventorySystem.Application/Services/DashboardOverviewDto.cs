using System.Collections.Generic;

namespace InventorySystem.Application.Services
{
    public class DashboardOverviewDto
    {
        public DashboardKpiDto Kpis { get; set; } = new();
        public List<DashboardModuleDto> Modules { get; set; } = new();
        public List<string> Integrations { get; set; } = new();
        public List<string> NextMilestones { get; set; } = new();
        public List<string> RemainingGaps { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class DashboardKpiDto
    {
        public int TotalProducts { get; set; }
        public int ActiveWarehouses { get; set; }
        public int PendingAdjustments { get; set; }
        public int LowStockItems { get; set; }
        public decimal InventoryValueEstimate { get; set; }
    }

    public class DashboardModuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
    }
}
