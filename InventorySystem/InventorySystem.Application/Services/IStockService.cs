using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public interface IStockService
    {
        Task<bool> PerformStockMovementAsync(int productId, int binId, decimal amount, string reasonCode, string referenceDocumentId, CancellationToken cancellationToken = default);
    }
}
