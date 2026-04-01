using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public class AdjustmentService : IAdjustmentService
    {
        private readonly IApplicationDbContext _context;
        private readonly IStockService _stockService;

        public AdjustmentService(IApplicationDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public async Task<AdjustmentRequest> RequestAdjustmentAsync(int productId, decimal requestedAmount, string reason, string requestedBy, CancellationToken cancellationToken = default)
        {
            var request = new AdjustmentRequest
            {
                ProductId = productId,
                RequestedAmount = requestedAmount,
                Reason = reason,
                RequestedBy = requestedBy,
                Status = AdjustmentStatus.Pending
            };

            _context.AdjustmentRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);
            return request;
        }

        public async Task<bool> ApproveAdjustmentAsync(int requestId, string approvedBy, CancellationToken cancellationToken = default)
        {
            var request = await _context.AdjustmentRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == AdjustmentStatus.Pending, cancellationToken);
            
            if (request == null) return false;

            var bin = await _context.WarehouseBins.FirstOrDefaultAsync(cancellationToken);
            if (bin == null) return false; // In a robust app, bin ID should be captured in request

            var success = await _stockService.PerformStockMovementAsync(request.ProductId, bin.Id, request.RequestedAmount, "ADJUSTMENT-APP", $"ADJ-{request.Id}", cancellationToken);
            
            if (success)
            {
                request.Status = AdjustmentStatus.Approved;
                request.ApprovedBy = approvedBy;
                request.ApprovedAt = DateTime.UtcNow;
                _context.AdjustmentRequests.Update(request);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return success;
        }

        public async Task<bool> RejectAdjustmentAsync(int requestId, string rejectedBy, CancellationToken cancellationToken = default)
        {
            var request = await _context.AdjustmentRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == AdjustmentStatus.Pending, cancellationToken);
            
            if (request == null) return false;

            request.Status = AdjustmentStatus.Rejected;
            request.ApprovedBy = rejectedBy; 
            request.ApprovedAt = DateTime.UtcNow;
            
            _context.AdjustmentRequests.Update(request);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<AdjustmentRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AdjustmentRequests
                .Include(r => r.Product)
                .Where(r => r.Status == AdjustmentStatus.Pending)
                .ToListAsync(cancellationToken);
        }
    }
}
