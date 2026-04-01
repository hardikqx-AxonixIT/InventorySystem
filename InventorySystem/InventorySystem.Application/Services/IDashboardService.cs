using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public interface IDashboardService
    {
        Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    }
}
