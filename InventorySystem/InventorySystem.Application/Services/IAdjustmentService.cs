using InventorySystem.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public interface IAdjustmentService
    {
        Task<AdjustmentRequest> RequestAdjustmentAsync(int productId, decimal requestedAmount, string reason, string requestedBy, CancellationToken cancellationToken = default);
        Task<bool> ApproveAdjustmentAsync(int requestId, string approvedBy, CancellationToken cancellationToken = default);
        Task<bool> RejectAdjustmentAsync(int requestId, string rejectedBy, CancellationToken cancellationToken = default);
        Task<List<AdjustmentRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    }
}
