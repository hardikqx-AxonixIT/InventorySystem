using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IApplicationDbContext _context;

        public StockService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PerformStockMovementAsync(int productId, int binId, decimal amount, string reasonCode, string referenceDocumentId, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var stockLevel = await _context.StockLevels
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.BinId == binId, cancellationToken);
                
                if (stockLevel == null)
                {
                    if (amount < 0) return false; // Cannot deduct from non-existent stock

                    stockLevel = new StockLevel
                    {
                        ProductId = productId,
                        BinId = binId,
                        QuantityOnHand = amount,
                        ReservedQuantity = 0
                    };
                    _context.StockLevels.Add(stockLevel);
                }
                else
                {
                    if (stockLevel.QuantityOnHand + amount < 0) return false; // Prevent negative stock
                    
                    stockLevel.QuantityOnHand += amount;
                    _context.StockLevels.Update(stockLevel); // Flags for concurrency check
                }

                var ledger = new StockLedger
                {
                    ProductId = productId,
                    ChangeAmount = amount,
                    PostChangeBalance = stockLevel.QuantityOnHand,
                    ReasonCode = reasonCode,
                    ReferenceDocumentId = referenceDocumentId
                };
                
                _context.StockLedgers.Add(ledger);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw; // Caller handles concurrency specifically
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
