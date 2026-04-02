using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IApplicationDbContext _context;
        private readonly IStockService _stockService;

        public TransactionService(IApplicationDbContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public async Task<object> GetBootstrapAsync(CancellationToken cancellationToken = default)
        {
            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync(cancellationToken);
            var customers = await _context.Customers.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            var vendors = await _context.Vendors.OrderBy(v => v.Name).ToListAsync(cancellationToken);
            var warehouses = await _context.Warehouses.OrderBy(w => w.Name).ToListAsync(cancellationToken);
            var bins = await _context.WarehouseBins.OrderBy(b => b.BinCode).ToListAsync(cancellationToken);
            var stockLevels = await _context.StockLevels
                .OrderBy(s => s.ProductId)
                .ThenBy(s => s.BinId)
                .Take(200)
                .ToListAsync(cancellationToken);

            var purchaseOrders = await _context.PurchaseOrders
                .Include(p => p.Items)
                .OrderByDescending(p => p.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            var grns = await _context.GoodsReceiptNotes
                .Include(g => g.Items)
                .OrderByDescending(g => g.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            var purchaseInvoices = await _context.PurchaseInvoices
                .Include(p => p.Items)
                .OrderByDescending(p => p.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            var supplierPayments = await _context.SupplierPayments
                .OrderByDescending(p => p.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            var salesOrders = await _context.SalesOrders
                .Include(s => s.Items)
                .OrderByDescending(s => s.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            var invoices = await _context.SalesInvoices
                .Include(s => s.Items)
                .OrderByDescending(s => s.Id)
                .Take(20)
                .ToListAsync(cancellationToken);

            var gstSummary = new GstSummaryDto
            {
                PurchaseCgst = purchaseInvoices.Sum(x => x.CgstAmount),
                PurchaseSgst = purchaseInvoices.Sum(x => x.SgstAmount),
                PurchaseIgst = purchaseInvoices.Sum(x => x.IgstAmount),
                SalesCgst = invoices.Sum(x => x.CgstAmount),
                SalesSgst = invoices.Sum(x => x.SgstAmount),
                SalesIgst = invoices.Sum(x => x.IgstAmount)
            };
            gstSummary.NetGstPayable = (gstSummary.SalesCgst + gstSummary.SalesSgst + gstSummary.SalesIgst)
                - (gstSummary.PurchaseCgst + gstSummary.PurchaseSgst + gstSummary.PurchaseIgst);
            var accountingSummary = BuildAccountingSummary(purchaseInvoices, supplierPayments, invoices, gstSummary);

            var productNameById = products.ToDictionary(p => p.Id, p => p.Name);
            var binById = bins.ToDictionary(b => b.Id);

            return new
            {
                products,
                customers,
                vendors,
                warehouses,
                bins,
                stockLevels = stockLevels.Select(s => new
                {
                    s.ProductId,
                    ProductName = productNameById.ContainsKey(s.ProductId) ? productNameById[s.ProductId] : null,
                    s.BinId,
                    BinCode = binById.ContainsKey(s.BinId) ? binById[s.BinId].BinCode : null,
                    WarehouseId = binById.ContainsKey(s.BinId) ? binById[s.BinId].WarehouseId : 0,
                    QuantityOnHand = s.QuantityOnHand,
                    ReservedQuantity = s.ReservedQuantity,
                    AvailableQuantity = s.QuantityOnHand - s.ReservedQuantity
                }),
                purchaseOrders = purchaseOrders.Select(p => new
                {
                    p.Id,
                    p.OrderNumber,
                    p.VendorId,
                    p.WarehouseId,
                    p.SupplyState,
                    p.Status,
                    p.Subtotal,
                    p.CgstAmount,
                    p.SgstAmount,
                    p.IgstAmount,
                    p.TaxTotal,
                    p.GrandTotal,
                    Items = p.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.Quantity,
                        i.ReceivedQuantity,
                        i.UnitPrice,
                        i.GstRate,
                        i.LineTotal
                    })
                }),
                goodsReceiptNotes = grns.Select(g => new
                {
                    g.Id,
                    g.GrnNumber,
                    g.PurchaseOrderId,
                    g.Subtotal,
                    g.CgstAmount,
                    g.SgstAmount,
                    g.IgstAmount,
                    g.GrandTotal,
                    Items = g.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.BinId,
                        i.QuantityReceived,
                        i.LineTotal
                    })
                }),
                purchaseInvoices = purchaseInvoices.Select(p => new
                {
                    p.Id,
                    p.InvoiceNumber,
                    p.GoodsReceiptNoteId,
                    p.PurchaseOrderId,
                    p.VendorId,
                    p.InvoiceDate,
                    p.DueDate,
                    p.Status,
                    p.Subtotal,
                    p.CgstAmount,
                    p.SgstAmount,
                    p.IgstAmount,
                    p.TaxTotal,
                    p.GrandTotal,
                    p.PaidAmount,
                    p.BalanceAmount,
                    Items = p.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.Quantity,
                        i.UnitPrice,
                        i.GstRate,
                        i.LineTotal
                    })
                }),
                supplierPayments = supplierPayments.Select(p => new
                {
                    p.Id,
                    p.PaymentNumber,
                    p.VendorId,
                    p.PurchaseInvoiceId,
                    p.PaymentDate,
                    p.Amount,
                    p.PaymentMode,
                    p.ReferenceNo,
                    p.Notes
                }),
                salesOrders = salesOrders.Select(s => new
                {
                    s.Id,
                    s.OrderNumber,
                    s.CustomerId,
                    s.WarehouseId,
                    s.PlaceOfSupplyState,
                    s.Status,
                    s.Subtotal,
                    s.CgstAmount,
                    s.SgstAmount,
                    s.IgstAmount,
                    s.TaxTotal,
                    s.GrandTotal,
                    Items = s.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.BinId,
                        i.Quantity,
                        i.InvoicedQuantity,
                        i.UnitPrice,
                        i.GstRate,
                        i.LineTotal
                    })
                }),
                salesInvoices = invoices.Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.SalesOrderId,
                    i.CustomerId,
                    i.WarehouseId,
                    i.PlaceOfSupplyState,
                    i.Subtotal,
                    i.CgstAmount,
                    i.SgstAmount,
                    i.IgstAmount,
                    i.TaxTotal,
                    i.GrandTotal,
                    Items = i.Items.Select(x => new
                    {
                        x.Id,
                        x.ProductId,
                        x.BinId,
                        x.Quantity,
                        x.LineTotal
                    })
                }),
                gstSummary,
                accountingSummary
            };
        }

        public async Task<object> GetPurchaseOrdersPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        {
            var safePage = page < 1 ? 1 : page;
            var safeSize = pageSize < 1 ? 25 : Math.Min(pageSize, 200);
            var query = _context.PurchaseOrders.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.Id);
            var total = await query.CountAsync(cancellationToken);
            var records = await query
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .Select(p => new
                {
                    p.Id,
                    p.OrderNumber,
                    p.VendorId,
                    p.WarehouseId,
                    p.SupplyState,
                    p.Status,
                    p.Subtotal,
                    p.CgstAmount,
                    p.SgstAmount,
                    p.IgstAmount,
                    p.TaxTotal,
                    p.GrandTotal,
                    Items = p.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.Quantity,
                        i.ReceivedQuantity,
                        i.UnitPrice,
                        i.GstRate,
                        i.LineTotal
                    })
                })
                .ToListAsync(cancellationToken);
            return new { page = safePage, pageSize = safeSize, total, records };
        }

        public async Task<object> GetGoodsReceiptsPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        {
            var safePage = page < 1 ? 1 : page;
            var safeSize = pageSize < 1 ? 25 : Math.Min(pageSize, 200);
            var query = _context.GoodsReceiptNotes.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.Id);
            var total = await query.CountAsync(cancellationToken);
            var records = await query
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .Select(g => new
                {
                    g.Id,
                    g.GrnNumber,
                    g.PurchaseOrderId,
                    g.Subtotal,
                    g.CgstAmount,
                    g.SgstAmount,
                    g.IgstAmount,
                    g.GrandTotal,
                    Items = g.Items.Select(i => new
                    {
                        i.Id,
                        i.PurchaseOrderItemId,
                        i.ProductId,
                        i.BinId,
                        i.QuantityReceived,
                        i.LineTotal
                    })
                })
                .ToListAsync(cancellationToken);
            return new { page = safePage, pageSize = safeSize, total, records };
        }

        public async Task<object> GetPurchaseInvoicesPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        {
            var safePage = page < 1 ? 1 : page;
            var safeSize = pageSize < 1 ? 25 : Math.Min(pageSize, 200);
            var query = _context.PurchaseInvoices.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.Id);
            var total = await query.CountAsync(cancellationToken);
            var records = await query
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .Select(p => new
                {
                    p.Id,
                    p.InvoiceNumber,
                    p.GoodsReceiptNoteId,
                    p.PurchaseOrderId,
                    p.VendorId,
                    p.InvoiceDate,
                    p.DueDate,
                    p.Status,
                    p.Subtotal,
                    p.CgstAmount,
                    p.SgstAmount,
                    p.IgstAmount,
                    p.TaxTotal,
                    p.GrandTotal,
                    p.PaidAmount,
                    p.BalanceAmount,
                    Items = p.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.Quantity,
                        i.UnitPrice,
                        i.GstRate,
                        i.LineTotal
                    })
                })
                .ToListAsync(cancellationToken);
            return new { page = safePage, pageSize = safeSize, total, records };
        }

        public async Task<object> GetSalesOrdersPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        {
            var safePage = page < 1 ? 1 : page;
            var safeSize = pageSize < 1 ? 25 : Math.Min(pageSize, 200);
            var query = _context.SalesOrders.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.Id);
            var total = await query.CountAsync(cancellationToken);
            var records = await query
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .Select(s => new
                {
                    s.Id,
                    s.OrderNumber,
                    s.CustomerId,
                    s.WarehouseId,
                    s.PlaceOfSupplyState,
                    s.Status,
                    s.Subtotal,
                    s.CgstAmount,
                    s.SgstAmount,
                    s.IgstAmount,
                    s.TaxTotal,
                    s.GrandTotal,
                    Items = s.Items.Select(i => new
                    {
                        i.Id,
                        i.ProductId,
                        i.BinId,
                        i.Quantity,
                        i.InvoicedQuantity,
                        i.UnitPrice,
                        i.GstRate,
                        i.LineTotal
                    })
                })
                .ToListAsync(cancellationToken);
            return new { page = safePage, pageSize = safeSize, total, records };
        }

        public async Task<object> GetSalesInvoicesPagedAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
        {
            var safePage = page < 1 ? 1 : page;
            var safeSize = pageSize < 1 ? 25 : Math.Min(pageSize, 200);
            var query = _context.SalesInvoices.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.Id);
            var total = await query.CountAsync(cancellationToken);
            var records = await query
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.SalesOrderId,
                    i.CustomerId,
                    i.WarehouseId,
                    i.PlaceOfSupplyState,
                    i.InvoiceDate,
                    i.Status,
                    i.Subtotal,
                    i.CgstAmount,
                    i.SgstAmount,
                    i.IgstAmount,
                    i.TaxTotal,
                    i.GrandTotal,
                    Items = i.Items.Select(x => new
                    {
                        x.Id,
                        x.ProductId,
                        x.BinId,
                        x.Quantity,
                        x.LineTotal
                    })
                })
                .ToListAsync(cancellationToken);
            return new { page = safePage, pageSize = safeSize, total, records };
        }

        public async Task<object> CreatePurchaseOrderAsync(PurchaseOrderCreateDto request, CancellationToken cancellationToken = default)
        {
            var warehouse = await _context.Warehouses.FirstAsync(w => w.Id == request.WarehouseId, cancellationToken);
            var order = new PurchaseOrder
            {
                OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                VendorId = request.VendorId,
                WarehouseId = request.WarehouseId,
                SupplyState = string.IsNullOrWhiteSpace(request.SupplyState) ? warehouse.State : request.SupplyState
            };

            foreach (var item in request.Items)
            {
                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, order.SupplyState, warehouse.State);
                order.Items.Add(new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    GstRate = item.GstRate,
                    TaxableAmount = tax.TaxableAmount,
                    CgstAmount = tax.CgstAmount,
                    SgstAmount = tax.SgstAmount,
                    IgstAmount = tax.IgstAmount,
                    LineTotal = tax.LineTotal
                });
            }

            ApplyPurchaseTotals(order);
            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);
            return new
            {
                order.Id,
                order.OrderNumber,
                order.VendorId,
                order.WarehouseId,
                order.SupplyState,
                order.Status,
                order.Subtotal,
                order.CgstAmount,
                order.SgstAmount,
                order.IgstAmount,
                order.TaxTotal,
                order.GrandTotal,
                items = order.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.GstRate,
                    i.LineTotal
                })
            };
        }

        public async Task<object> UpdatePurchaseOrderAsync(int purchaseOrderId, PurchaseOrderUpdateDto request, CancellationToken cancellationToken = default)
        {
            var order = await _context.PurchaseOrders
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                .FirstAsync(x => x.Id == purchaseOrderId, cancellationToken);

            if (order.Status == DocumentStatus.Cancelled || order.Status == DocumentStatus.Completed)
            {
                throw new InvalidOperationException("Only open purchase orders can be updated.");
            }

            if (order.Items.Any(x => x.ReceivedQuantity > 0))
            {
                throw new InvalidOperationException("Purchase order cannot be edited after GRN posting.");
            }

            var warehouse = await _context.Warehouses.FirstAsync(w => w.Id == request.WarehouseId, cancellationToken);

            _context.PurchaseOrderItems.RemoveRange(order.Items);
            order.Items.Clear();
            order.VendorId = request.VendorId;
            order.WarehouseId = request.WarehouseId;
            order.SupplyState = string.IsNullOrWhiteSpace(request.SupplyState) ? warehouse.State : request.SupplyState;

            foreach (var item in request.Items)
            {
                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, order.SupplyState, warehouse.State);
                order.Items.Add(new PurchaseOrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    GstRate = item.GstRate,
                    TaxableAmount = tax.TaxableAmount,
                    CgstAmount = tax.CgstAmount,
                    SgstAmount = tax.SgstAmount,
                    IgstAmount = tax.IgstAmount,
                    LineTotal = tax.LineTotal
                });
            }

            ApplyPurchaseTotals(order);
            await _context.SaveChangesAsync(cancellationToken);
            return new { order.Id, order.OrderNumber, order.Status, order.GrandTotal };
        }

        public async Task<object> CancelPurchaseOrderAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
        {
            var order = await _context.PurchaseOrders.Include(x => x.Items).FirstAsync(x => x.Id == purchaseOrderId, cancellationToken);
            if (order.Items.Any(x => x.ReceivedQuantity > 0))
            {
                throw new InvalidOperationException("Purchase order with GRN cannot be cancelled.");
            }

            order.Status = DocumentStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
            return new { order.Id, order.OrderNumber, order.Status };
        }

        public async Task<object> ReceiveGoodsAsync(GoodsReceiptCreateDto request, CancellationToken cancellationToken = default)
        {
            var order = await _context.PurchaseOrders
                .Include(p => p.Warehouse)
                .Include(p => p.Items)
                .FirstAsync(p => p.Id == request.PurchaseOrderId, cancellationToken);

            var grn = new GoodsReceiptNote
            {
                PurchaseOrderId = order.Id,
                GrnNumber = $"GRN-{DateTime.UtcNow:yyyyMMddHHmmss}"
            };

            foreach (var receiveLine in request.Items.Where(i => i.QuantityReceived > 0))
            {
                var orderItem = order.Items.First(x => x.Id == receiveLine.PurchaseOrderItemId);
                var outstanding = orderItem.Quantity - orderItem.ReceivedQuantity;
                if (receiveLine.QuantityReceived > outstanding)
                {
                    throw new InvalidOperationException("Received quantity cannot exceed outstanding quantity.");
                }

                var tax = CalculateTax(receiveLine.QuantityReceived, orderItem.UnitPrice, orderItem.GstRate, order.SupplyState, order.Warehouse?.State);

                grn.Items.Add(new GoodsReceiptNoteItem
                {
                    PurchaseOrderItemId = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    BinId = receiveLine.BinId,
                    QuantityReceived = receiveLine.QuantityReceived,
                    UnitPrice = orderItem.UnitPrice,
                    GstRate = orderItem.GstRate,
                    TaxableAmount = tax.TaxableAmount,
                    CgstAmount = tax.CgstAmount,
                    SgstAmount = tax.SgstAmount,
                    IgstAmount = tax.IgstAmount,
                    LineTotal = tax.LineTotal
                });

                orderItem.ReceivedQuantity += receiveLine.QuantityReceived;
                await _stockService.PerformStockMovementAsync(orderItem.ProductId, receiveLine.BinId, receiveLine.QuantityReceived, "PURCHASE_GRN", grn.GrnNumber, cancellationToken);
            }

            grn.Subtotal = grn.Items.Sum(i => i.TaxableAmount);
            grn.CgstAmount = grn.Items.Sum(i => i.CgstAmount);
            grn.SgstAmount = grn.Items.Sum(i => i.SgstAmount);
            grn.IgstAmount = grn.Items.Sum(i => i.IgstAmount);
            grn.TaxTotal = grn.CgstAmount + grn.SgstAmount + grn.IgstAmount;
            grn.GrandTotal = grn.Subtotal + grn.TaxTotal;

            order.Status = order.Items.All(i => i.ReceivedQuantity >= i.Quantity)
                ? DocumentStatus.Completed
                : DocumentStatus.PartiallyProcessed;

            _context.GoodsReceiptNotes.Add(grn);
            await _context.SaveChangesAsync(cancellationToken);
            return new
            {
                grn.Id,
                grn.GrnNumber,
                grn.PurchaseOrderId,
                grn.Status,
                grn.Subtotal,
                grn.CgstAmount,
                grn.SgstAmount,
                grn.IgstAmount,
                grn.TaxTotal,
                grn.GrandTotal,
                items = grn.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.BinId,
                    i.QuantityReceived,
                    i.LineTotal
                })
            };
        }

        public async Task<object> CreatePurchaseInvoiceAsync(PurchaseInvoiceCreateDto request, CancellationToken cancellationToken = default)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.PurchaseOrder)
                .Include(g => g.Items)
                .FirstAsync(g => g.Id == request.GoodsReceiptNoteId, cancellationToken);

            if (await _context.PurchaseInvoices.AnyAsync(p => p.GoodsReceiptNoteId == grn.Id, cancellationToken))
            {
                throw new InvalidOperationException("Purchase invoice already exists for selected GRN.");
            }

            var purchaseOrder = grn.PurchaseOrder ?? await _context.PurchaseOrders.FirstAsync(p => p.Id == grn.PurchaseOrderId, cancellationToken);

            var invoice = new PurchaseInvoice
            {
                InvoiceNumber = $"PINV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                GoodsReceiptNoteId = grn.Id,
                PurchaseOrderId = purchaseOrder.Id,
                VendorId = purchaseOrder.VendorId,
                InvoiceDate = DateTime.UtcNow,
                DueDate = request.DueDate,
                Status = DocumentStatus.Open
            };

            foreach (var line in grn.Items)
            {
                invoice.Items.Add(new PurchaseInvoiceItem
                {
                    GoodsReceiptNoteItemId = line.Id,
                    ProductId = line.ProductId,
                    Quantity = line.QuantityReceived,
                    UnitPrice = line.UnitPrice,
                    GstRate = line.GstRate,
                    TaxableAmount = line.TaxableAmount,
                    CgstAmount = line.CgstAmount,
                    SgstAmount = line.SgstAmount,
                    IgstAmount = line.IgstAmount,
                    LineTotal = line.LineTotal
                });
            }

            invoice.Subtotal = invoice.Items.Sum(i => i.TaxableAmount);
            invoice.CgstAmount = invoice.Items.Sum(i => i.CgstAmount);
            invoice.SgstAmount = invoice.Items.Sum(i => i.SgstAmount);
            invoice.IgstAmount = invoice.Items.Sum(i => i.IgstAmount);
            invoice.TaxTotal = invoice.CgstAmount + invoice.SgstAmount + invoice.IgstAmount;
            invoice.GrandTotal = invoice.Subtotal + invoice.TaxTotal;
            invoice.PaidAmount = 0;
            invoice.BalanceAmount = invoice.GrandTotal;

            _context.PurchaseInvoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);
            return new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.GoodsReceiptNoteId,
                invoice.PurchaseOrderId,
                invoice.VendorId,
                invoice.InvoiceDate,
                invoice.DueDate,
                invoice.Status,
                invoice.Subtotal,
                invoice.CgstAmount,
                invoice.SgstAmount,
                invoice.IgstAmount,
                invoice.TaxTotal,
                invoice.GrandTotal,
                invoice.PaidAmount,
                invoice.BalanceAmount,
                items = invoice.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.GstRate,
                    i.LineTotal
                })
            };
        }

        public async Task<object> CreateSupplierPaymentAsync(SupplierPaymentCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.Amount <= 0)
            {
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            var invoice = await _context.PurchaseInvoices.FirstAsync(p => p.Id == request.PurchaseInvoiceId, cancellationToken);
            if (request.Amount > invoice.BalanceAmount)
            {
                throw new InvalidOperationException("Payment amount cannot exceed invoice balance.");
            }

            var payment = new SupplierPayment
            {
                PaymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
                VendorId = invoice.VendorId,
                PurchaseInvoiceId = invoice.Id,
                Amount = request.Amount,
                PaymentMode = request.PaymentMode?.Trim(),
                ReferenceNo = request.ReferenceNo?.Trim(),
                Notes = request.Notes?.Trim()
            };

            invoice.PaidAmount += request.Amount;
            invoice.BalanceAmount -= request.Amount;
            invoice.Status = invoice.BalanceAmount <= 0
                ? DocumentStatus.Completed
                : DocumentStatus.PartiallyProcessed;

            _context.SupplierPayments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);
            return new
            {
                payment.Id,
                payment.PaymentNumber,
                payment.VendorId,
                payment.PurchaseInvoiceId,
                payment.PaymentDate,
                payment.Amount,
                payment.PaymentMode,
                payment.ReferenceNo,
                payment.Notes
            };
        }

        public async Task<object> CreateStockInAsync(StockMovementCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                throw new InvalidOperationException("Stock-in quantity must be greater than zero.");
            }

            var reference = string.IsNullOrWhiteSpace(request.ReferenceNo)
                ? $"IN-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : request.ReferenceNo.Trim();

            var success = await _stockService.PerformStockMovementAsync(request.ProductId, request.BinId, request.Quantity, "STOCK_IN", reference, cancellationToken);
            if (!success)
            {
                throw new InvalidOperationException("Unable to post stock-in.");
            }

            var stock = await _context.StockLevels.FirstAsync(s => s.ProductId == request.ProductId && s.BinId == request.BinId, cancellationToken);
            return new
            {
                request.ProductId,
                request.BinId,
                Movement = request.Quantity,
                stock.QuantityOnHand,
                stock.ReservedQuantity,
                AvailableQuantity = stock.QuantityOnHand - stock.ReservedQuantity
            };
        }

        public async Task<object> CreateStockOutAsync(StockMovementCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                throw new InvalidOperationException("Stock-out quantity must be greater than zero.");
            }

            var reference = string.IsNullOrWhiteSpace(request.ReferenceNo)
                ? $"OUT-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : request.ReferenceNo.Trim();

            var success = await _stockService.PerformStockMovementAsync(request.ProductId, request.BinId, -request.Quantity, "STOCK_OUT", reference, cancellationToken);
            if (!success)
            {
                throw new InvalidOperationException("Insufficient stock for stock-out.");
            }

            var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.BinId == request.BinId, cancellationToken);
            return new
            {
                request.ProductId,
                request.BinId,
                Movement = -request.Quantity,
                QuantityOnHand = stock?.QuantityOnHand ?? 0,
                ReservedQuantity = stock?.ReservedQuantity ?? 0,
                AvailableQuantity = (stock?.QuantityOnHand ?? 0) - (stock?.ReservedQuantity ?? 0)
            };
        }

        public async Task<object> CreateStockTransferAsync(StockTransferCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                throw new InvalidOperationException("Transfer quantity must be greater than zero.");
            }

            if (request.FromBinId == request.ToBinId)
            {
                throw new InvalidOperationException("Source and destination bin cannot be same.");
            }

            var reference = string.IsNullOrWhiteSpace(request.ReferenceNo)
                ? $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : request.ReferenceNo.Trim();

            var deductSuccess = await _stockService.PerformStockMovementAsync(request.ProductId, request.FromBinId, -request.Quantity, "STOCK_TRANSFER_OUT", reference, cancellationToken);
            if (!deductSuccess)
            {
                throw new InvalidOperationException("Insufficient stock in source bin.");
            }

            var addSuccess = await _stockService.PerformStockMovementAsync(request.ProductId, request.ToBinId, request.Quantity, "STOCK_TRANSFER_IN", reference, cancellationToken);
            if (!addSuccess)
            {
                throw new InvalidOperationException("Unable to post stock to destination bin.");
            }

            return new
            {
                request.ProductId,
                request.FromBinId,
                request.ToBinId,
                request.Quantity,
                ReferenceNo = reference
            };
        }

        public async Task<object> CreateProductionRunAsync(ProductionRunCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.OutputQuantity <= 0)
            {
                throw new InvalidOperationException("Output quantity must be greater than zero.");
            }

            if (request.Components.Count == 0)
            {
                throw new InvalidOperationException("At least one component is required.");
            }

            var reference = string.IsNullOrWhiteSpace(request.ReferenceNo)
                ? $"MFG-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : request.ReferenceNo.Trim();

            foreach (var component in request.Components)
            {
                if (component.Quantity <= 0)
                {
                    throw new InvalidOperationException("Component quantity must be greater than zero.");
                }

                var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == component.ProductId && s.BinId == component.BinId, cancellationToken);
                if (stock == null || stock.QuantityOnHand - stock.ReservedQuantity < component.Quantity)
                {
                    throw new InvalidOperationException("Insufficient component stock for manufacturing run.");
                }
            }

            foreach (var component in request.Components)
            {
                await _stockService.PerformStockMovementAsync(component.ProductId, component.BinId, -component.Quantity, "MFG_CONSUME", reference, cancellationToken);
            }

            await _stockService.PerformStockMovementAsync(request.FinishedProductId, request.OutputBinId, request.OutputQuantity, "MFG_OUTPUT", reference, cancellationToken);

            return new
            {
                ReferenceNo = reference,
                request.FinishedProductId,
                request.OutputBinId,
                request.OutputQuantity,
                Components = request.Components
            };
        }

        public async Task<object> GetReportsSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var products = await _context.Products.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            var stockLevels = await _context.StockLevels.ToListAsync(cancellationToken);
            var salesInvoiceItems = await _context.SalesInvoiceItems.ToListAsync(cancellationToken);
            var purchaseInvoices = await _context.PurchaseInvoices.ToListAsync(cancellationToken);
            var salesInvoices = await _context.SalesInvoices.ToListAsync(cancellationToken);

            var lowStock = products.Select(product =>
            {
                var available = stockLevels.Where(s => s.ProductId == product.Id).Sum(s => s.QuantityOnHand - s.ReservedQuantity);
                return new
                {
                    product.Id,
                    product.Name,
                    product.SKU,
                    product.ReorderLevel,
                    Available = available
                };
            })
            .Where(x => x.Available <= x.ReorderLevel)
            .OrderBy(x => x.Available)
            .Take(20)
            .ToList();

            var fastMoving = salesInvoiceItems
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Qty)
                .Take(10)
                .ToList()
                .Select(x => new
                {
                    x.ProductId,
                    ProductName = products.FirstOrDefault(p => p.Id == x.ProductId)?.Name,
                    x.Qty
                });

            var slowMoving = products
                .Select(p => new
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Qty = salesInvoiceItems.Where(x => x.ProductId == p.Id).Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Qty)
                .Take(10);

            var salesTotal = salesInvoices.Sum(x => x.GrandTotal);
            var purchaseTotal = purchaseInvoices.Sum(x => x.GrandTotal);

            var last7Days = Enumerable.Range(0, 7).Select(d => DateTime.UtcNow.Date.AddDays(-d)).Reverse();
            var salesTrend = last7Days.Select(date => new
            {
                Date = date.ToString("MMM dd"),
                Total = salesInvoices.Where(x => x.InvoiceDate.Date == date).Sum(x => x.GrandTotal)
            });

            var purchaseTrend = last7Days.Select(date => new
            {
                Date = date.ToString("MMM dd"),
                Total = purchaseInvoices.Where(x => x.InvoiceDate.Date == date).Sum(x => x.GrandTotal)
            });

            return new
            {
                salesTotal,
                purchaseTotal,
                grossMarginEstimate = salesTotal - purchaseTotal,
                lowStock,
                fastMoving,
                slowMoving,
                salesTrend,
                purchaseTrend
            };
        }

        public async Task<object> GetMobileDashboardAsync(CancellationToken cancellationToken = default)
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            var salesInvoices = await _context.SalesInvoices.ToListAsync(cancellationToken);
            var purchaseInvoices = await _context.PurchaseInvoices.ToListAsync(cancellationToken);
            var supplierPayments = await _context.SupplierPayments.ToListAsync(cancellationToken);
            var products = await _context.Products.ToListAsync(cancellationToken);
            var stockLevels = await _context.StockLevels.ToListAsync(cancellationToken);

            var todaySales = salesInvoices
                .Where(x => x.InvoiceDate >= todayStart && x.InvoiceDate < todayEnd)
                .Sum(x => x.GrandTotal);

            var outstandingPayables = purchaseInvoices.Sum(x => x.BalanceAmount);
            var todaySupplierPayments = supplierPayments
                .Where(x => x.PaymentDate >= todayStart && x.PaymentDate < todayEnd)
                .Sum(x => x.Amount);

            var lowStockCount = products.Count(product =>
            {
                var available = stockLevels.Where(s => s.ProductId == product.Id).Sum(s => s.QuantityOnHand - s.ReservedQuantity);
                return available <= product.ReorderLevel;
            });

            return new
            {
                todaySales,
                lowStockCount,
                outstandingPayables,
                todaySupplierPayments
            };
        }

        public async Task<object> GetDemandPredictionAsync(int lookbackDays = 30, CancellationToken cancellationToken = default)
        {
            var fromDate = DateTime.UtcNow.Date.AddDays(-lookbackDays);
            var products = await _context.Products.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            var salesLines = await _context.SalesInvoiceItems
                .Where(x => x.SalesInvoice != null && x.SalesInvoice.InvoiceDate >= fromDate)
                .Include(x => x.SalesInvoice)
                .ToListAsync(cancellationToken);

            var predictions = products.Select(product =>
            {
                var totalQty = salesLines.Where(x => x.ProductId == product.Id).Sum(x => x.Quantity);
                var avgDaily = lookbackDays > 0 ? decimal.Round(totalQty / lookbackDays, 2) : 0m;
                var next15Days = decimal.Round(avgDaily * 15m, 2);
                return new
                {
                    product.Id,
                    product.SKU,
                    product.Name,
                    HistoricalQty = totalQty,
                    AvgDailyDemand = avgDaily,
                    ForecastNext15Days = next15Days
                };
            })
            .OrderByDescending(x => x.ForecastNext15Days)
            .ToList();

            return new
            {
                LookbackDays = lookbackDays,
                Predictions = predictions
            };
        }

        public async Task<object> GetAutoReorderSuggestionsAsync(int lookbackDays = 30, int horizonDays = 15, CancellationToken cancellationToken = default)
        {
            var fromDate = DateTime.UtcNow.Date.AddDays(-lookbackDays);
            var products = await _context.Products.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            var stockLevels = await _context.StockLevels.ToListAsync(cancellationToken);
            var salesLines = await _context.SalesInvoiceItems
                .Where(x => x.SalesInvoice != null && x.SalesInvoice.InvoiceDate >= fromDate)
                .Include(x => x.SalesInvoice)
                .ToListAsync(cancellationToken);

            var suggestions = products.Select(product =>
            {
                var totalQty = salesLines.Where(x => x.ProductId == product.Id).Sum(x => x.Quantity);
                var avgDaily = lookbackDays > 0 ? totalQty / lookbackDays : 0m;
                var demandHorizon = decimal.Round(avgDaily * horizonDays, 2);
                var available = stockLevels.Where(x => x.ProductId == product.Id).Sum(x => x.QuantityOnHand - x.ReservedQuantity);
                var targetStock = Math.Max(product.ReorderLevel, demandHorizon);
                var reorderQty = Math.Max(0m, decimal.Round(targetStock - available, 2));

                return new
                {
                    product.Id,
                    product.SKU,
                    product.Name,
                    Available = available,
                    product.ReorderLevel,
                    ForecastDemand = demandHorizon,
                    SuggestedReorderQty = reorderQty
                };
            })
            .Where(x => x.SuggestedReorderQty > 0)
            .OrderByDescending(x => x.SuggestedReorderQty)
            .ToList();

            return new
            {
                LookbackDays = lookbackDays,
                HorizonDays = horizonDays,
                Suggestions = suggestions
            };
        }

        public async Task<object> GetAccountingSummaryAsync(CancellationToken cancellationToken = default)
        {
            var purchaseInvoices = await _context.PurchaseInvoices.OrderByDescending(p => p.Id).Take(100).ToListAsync(cancellationToken);
            var supplierPayments = await _context.SupplierPayments.OrderByDescending(s => s.Id).Take(100).ToListAsync(cancellationToken);
            var salesInvoices = await _context.SalesInvoices.OrderByDescending(s => s.Id).Take(100).ToListAsync(cancellationToken);

            var gstSummary = new GstSummaryDto
            {
                PurchaseCgst = purchaseInvoices.Sum(x => x.CgstAmount),
                PurchaseSgst = purchaseInvoices.Sum(x => x.SgstAmount),
                PurchaseIgst = purchaseInvoices.Sum(x => x.IgstAmount),
                SalesCgst = salesInvoices.Sum(x => x.CgstAmount),
                SalesSgst = salesInvoices.Sum(x => x.SgstAmount),
                SalesIgst = salesInvoices.Sum(x => x.IgstAmount)
            };
            gstSummary.NetGstPayable = (gstSummary.SalesCgst + gstSummary.SalesSgst + gstSummary.SalesIgst)
                - (gstSummary.PurchaseCgst + gstSummary.PurchaseSgst + gstSummary.PurchaseIgst);

            return BuildAccountingSummary(purchaseInvoices, supplierPayments, salesInvoices, gstSummary);
        }

        public async Task<object> CreateSalesOrderAsync(SalesOrderCreateDto request, CancellationToken cancellationToken = default)
        {
            var warehouse = await _context.Warehouses.FirstAsync(w => w.Id == request.WarehouseId, cancellationToken);
            var order = new SalesOrder
            {
                OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                CustomerId = request.CustomerId,
                WarehouseId = request.WarehouseId,
                PlaceOfSupplyState = string.IsNullOrWhiteSpace(request.PlaceOfSupplyState) ? warehouse.State : request.PlaceOfSupplyState
            };

            foreach (var item in request.Items)
            {
                var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.BinId == item.BinId, cancellationToken);
                if (stock == null || stock.QuantityOnHand - stock.ReservedQuantity < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient available stock for selected bin.");
                }

                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, warehouse.State, order.PlaceOfSupplyState);
                order.Items.Add(new SalesOrderItem
                {
                    ProductId = item.ProductId,
                    BinId = item.BinId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    GstRate = item.GstRate,
                    TaxableAmount = tax.TaxableAmount,
                    CgstAmount = tax.CgstAmount,
                    SgstAmount = tax.SgstAmount,
                    IgstAmount = tax.IgstAmount,
                    LineTotal = tax.LineTotal
                });

                stock.ReservedQuantity += item.Quantity;
            }

            order.Subtotal = order.Items.Sum(i => i.TaxableAmount);
            order.CgstAmount = order.Items.Sum(i => i.CgstAmount);
            order.SgstAmount = order.Items.Sum(i => i.SgstAmount);
            order.IgstAmount = order.Items.Sum(i => i.IgstAmount);
            order.TaxTotal = order.CgstAmount + order.SgstAmount + order.IgstAmount;
            order.GrandTotal = order.Subtotal + order.TaxTotal;

            _context.SalesOrders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);
            return new
            {
                order.Id,
                order.OrderNumber,
                order.CustomerId,
                order.WarehouseId,
                order.PlaceOfSupplyState,
                order.Status,
                order.Subtotal,
                order.CgstAmount,
                order.SgstAmount,
                order.IgstAmount,
                order.TaxTotal,
                order.GrandTotal,
                items = order.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.BinId,
                    i.Quantity,
                    i.UnitPrice,
                    i.GstRate,
                    i.LineTotal
                })
            };
        }

        public async Task<object> UpdateSalesOrderAsync(int salesOrderId, SalesOrderUpdateDto request, CancellationToken cancellationToken = default)
        {
            var order = await _context.SalesOrders
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                .FirstAsync(x => x.Id == salesOrderId, cancellationToken);

            if (order.Status == DocumentStatus.Cancelled || order.Status == DocumentStatus.Completed)
            {
                throw new InvalidOperationException("Only open sales orders can be updated.");
            }

            if (order.Items.Any(x => x.InvoicedQuantity > 0))
            {
                throw new InvalidOperationException("Sales order cannot be edited after invoicing.");
            }

            foreach (var oldItem in order.Items)
            {
                var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == oldItem.ProductId && s.BinId == oldItem.BinId, cancellationToken);
                if (stock != null)
                {
                    stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - oldItem.Quantity);
                }
            }

            var warehouse = await _context.Warehouses.FirstAsync(w => w.Id == request.WarehouseId, cancellationToken);
            _context.SalesOrderItems.RemoveRange(order.Items);
            order.Items.Clear();
            order.CustomerId = request.CustomerId;
            order.WarehouseId = request.WarehouseId;
            order.PlaceOfSupplyState = string.IsNullOrWhiteSpace(request.PlaceOfSupplyState) ? warehouse.State : request.PlaceOfSupplyState;

            foreach (var item in request.Items)
            {
                var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.BinId == item.BinId, cancellationToken);
                if (stock == null || stock.QuantityOnHand - stock.ReservedQuantity < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient available stock for selected bin.");
                }

                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, warehouse.State, order.PlaceOfSupplyState);
                order.Items.Add(new SalesOrderItem
                {
                    ProductId = item.ProductId,
                    BinId = item.BinId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    GstRate = item.GstRate,
                    TaxableAmount = tax.TaxableAmount,
                    CgstAmount = tax.CgstAmount,
                    SgstAmount = tax.SgstAmount,
                    IgstAmount = tax.IgstAmount,
                    LineTotal = tax.LineTotal
                });

                stock.ReservedQuantity += item.Quantity;
            }

            order.Subtotal = order.Items.Sum(i => i.TaxableAmount);
            order.CgstAmount = order.Items.Sum(i => i.CgstAmount);
            order.SgstAmount = order.Items.Sum(i => i.SgstAmount);
            order.IgstAmount = order.Items.Sum(i => i.IgstAmount);
            order.TaxTotal = order.CgstAmount + order.SgstAmount + order.IgstAmount;
            order.GrandTotal = order.Subtotal + order.TaxTotal;

            await _context.SaveChangesAsync(cancellationToken);
            return new { order.Id, order.OrderNumber, order.Status, order.GrandTotal };
        }

        public async Task<object> CancelSalesOrderAsync(int salesOrderId, CancellationToken cancellationToken = default)
        {
            var order = await _context.SalesOrders
                .Include(x => x.Items)
                .FirstAsync(x => x.Id == salesOrderId, cancellationToken);

            if (order.Items.Any(x => x.InvoicedQuantity > 0))
            {
                throw new InvalidOperationException("Invoiced sales order cannot be cancelled.");
            }

            foreach (var item in order.Items)
            {
                var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.BinId == item.BinId, cancellationToken);
                if (stock != null)
                {
                    stock.ReservedQuantity = Math.Max(0, stock.ReservedQuantity - item.Quantity);
                }
            }

            order.Status = DocumentStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
            return new { order.Id, order.OrderNumber, order.Status };
        }

        public async Task<object> CreateSalesInvoiceAsync(SalesInvoiceCreateDto request, CancellationToken cancellationToken = default)
        {
            var order = await _context.SalesOrders
                .Include(s => s.Warehouse)
                .Include(s => s.Items)
                .FirstAsync(s => s.Id == request.SalesOrderId, cancellationToken);

            // Indian Business Logic: Credit & Aging Check
            var customer = await _context.Customers.FirstAsync(c => c.Id == order.CustomerId, cancellationToken);
            
            // Check 1: 90-Day Aging Lock (Common in India)
            var nintyDaysAgo = DateTime.UtcNow.AddDays(-90);
            var overdueInvoices = await _context.SalesInvoices
                .Where(i => i.CustomerId == order.CustomerId && i.InvoiceDate <= nintyDaysAgo)
                .AnyAsync(cancellationToken);
            
            if (overdueInvoices)
                throw new InvalidOperationException("Invoice Blocked: Customer has unpaid bills over 90 days old. Please clear outstanding first.");

            // Check 2: Credit Limit Check
            var totalOutstanding = await _context.SalesInvoices
                .Where(i => i.CustomerId == order.CustomerId)
                .SumAsync(i => i.GrandTotal, cancellationToken);

            if (customer.CreditLimit > 0 && totalOutstanding + (order.GrandTotal) > customer.CreditLimit)
                throw new InvalidOperationException($"Credit Limit Exceeded: Customer limit is ₹{customer.CreditLimit:N0}. Current outstanding ₹{totalOutstanding:N0}. Order Rejected.");


            var invoice = new SalesInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SalesOrderId = order.Id,
                CustomerId = order.CustomerId,
                WarehouseId = order.WarehouseId,
                PlaceOfSupplyState = order.PlaceOfSupplyState
            };

            foreach (var line in order.Items.Where(i => i.InvoicedQuantity < i.Quantity))
            {
                var qtyToInvoice = line.Quantity - line.InvoicedQuantity;
                invoice.Items.Add(new SalesInvoiceItem
                {
                    SalesOrderItemId = line.Id,
                    ProductId = line.ProductId,
                    BinId = line.BinId,
                    Quantity = qtyToInvoice,
                    UnitPrice = line.UnitPrice,
                    GstRate = line.GstRate,
                    TaxableAmount = line.TaxableAmount * (qtyToInvoice / line.Quantity),
                    CgstAmount = line.CgstAmount * (qtyToInvoice / line.Quantity),
                    SgstAmount = line.SgstAmount * (qtyToInvoice / line.Quantity),
                    IgstAmount = line.IgstAmount * (qtyToInvoice / line.Quantity),
                    LineTotal = line.LineTotal * (qtyToInvoice / line.Quantity)
                });

                await DeductReservedStockAsync(line.ProductId, line.BinId, qtyToInvoice, invoice.InvoiceNumber, cancellationToken);
                line.InvoicedQuantity += qtyToInvoice;
            }

            invoice.Subtotal = invoice.Items.Sum(i => i.TaxableAmount);
            invoice.CgstAmount = invoice.Items.Sum(i => i.CgstAmount);
            invoice.SgstAmount = invoice.Items.Sum(i => i.SgstAmount);
            invoice.IgstAmount = invoice.Items.Sum(i => i.IgstAmount);
            invoice.TaxTotal = invoice.CgstAmount + invoice.SgstAmount + invoice.IgstAmount;
            invoice.GrandTotal = invoice.Subtotal + invoice.TaxTotal;

            order.Status = DocumentStatus.Completed;
            _context.SalesInvoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);
            return new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.SalesOrderId,
                invoice.CustomerId,
                invoice.WarehouseId,
                invoice.PlaceOfSupplyState,
                invoice.InvoiceDate,
                invoice.Status,
                invoice.Subtotal,
                invoice.CgstAmount,
                invoice.SgstAmount,
                invoice.IgstAmount,
                invoice.TaxTotal,
                invoice.GrandTotal,
                items = invoice.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.BinId,
                    i.Quantity,
                    i.UnitPrice,
                    i.GstRate,
                    i.LineTotal
                })
            };
        }

        private async Task DeductReservedStockAsync(int productId, int binId, decimal quantity, string reference, CancellationToken cancellationToken)
        {
            var stock = await _context.StockLevels.FirstAsync(s => s.ProductId == productId && s.BinId == binId, cancellationToken);
            stock.ReservedQuantity -= quantity;
            await _stockService.PerformStockMovementAsync(productId, binId, -quantity, "SALES_INVOICE", reference, cancellationToken);
        }

        private static TaxBreakdownDto CalculateTax(decimal quantity, decimal unitPrice, decimal gstRate, string? originState, string? destinationState)
        {
            var taxable = Math.Round(quantity * unitPrice, 2);
            var totalTax = Math.Round(taxable * gstRate / 100m, 2);
            var isIntraState = string.Equals(originState?.Trim(), destinationState?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (isIntraState)
            {
                var halfTax = Math.Round(totalTax / 2m, 2);
                return new TaxBreakdownDto
                {
                    TaxableAmount = taxable,
                    CgstAmount = halfTax,
                    SgstAmount = totalTax - halfTax,
                    IgstAmount = 0,
                    LineTotal = taxable + totalTax
                };
            }

            return new TaxBreakdownDto
            {
                TaxableAmount = taxable,
                CgstAmount = 0,
                SgstAmount = 0,
                IgstAmount = totalTax,
                LineTotal = taxable + totalTax
            };
        }

        private static void ApplyPurchaseTotals(PurchaseOrder order)
        {
            order.Subtotal = order.Items.Sum(i => i.TaxableAmount);
            order.CgstAmount = order.Items.Sum(i => i.CgstAmount);
            order.SgstAmount = order.Items.Sum(i => i.SgstAmount);
            order.IgstAmount = order.Items.Sum(i => i.IgstAmount);
            order.TaxTotal = order.CgstAmount + order.SgstAmount + order.IgstAmount;
            order.GrandTotal = order.Subtotal + order.TaxTotal;
        }

        private static object BuildAccountingSummary(
            IReadOnlyCollection<PurchaseInvoice> purchaseInvoices,
            IReadOnlyCollection<SupplierPayment> supplierPayments,
            IReadOnlyCollection<SalesInvoice> salesInvoices,
            GstSummaryDto gstSummary)
        {
            var totalPurchase = purchaseInvoices.Sum(x => x.GrandTotal);
            var totalSales = salesInvoices.Sum(x => x.GrandTotal);
            var purchaseTax = purchaseInvoices.Sum(x => x.TaxTotal);
            var salesTax = salesInvoices.Sum(x => x.TaxTotal);
            var totalPayables = purchaseInvoices.Sum(x => x.BalanceAmount);
            var totalSupplierPayments = supplierPayments.Sum(x => x.Amount);
            var totalReceivables = totalSales;
            var netRevenueExTax = salesInvoices.Sum(x => x.Subtotal);
            var netCostExTax = purchaseInvoices.Sum(x => x.Subtotal);

            var ledger = salesInvoices.Select(x => new
            {
                Date = x.InvoiceDate,
                VoucherType = "Sales Invoice",
                VoucherNo = x.InvoiceNumber,
                Debit = 0m,
                Credit = x.GrandTotal
            })
            .Concat(purchaseInvoices.Select(x => new
            {
                Date = x.InvoiceDate,
                VoucherType = "Purchase Invoice",
                VoucherNo = x.InvoiceNumber,
                Debit = x.GrandTotal,
                Credit = 0m
            }))
            .Concat(supplierPayments.Select(x => new
            {
                Date = x.PaymentDate,
                VoucherType = "Supplier Payment",
                VoucherNo = x.PaymentNumber,
                Debit = 0m,
                Credit = x.Amount
            }))
            .OrderByDescending(x => x.Date)
            .Take(30)
            .ToList();

            return new
            {
                totalSales,
                totalPurchase,
                totalReceivables,
                totalPayables,
                totalSupplierPayments,
                gstInput = purchaseTax,
                gstOutput = salesTax,
                gstNetPayable = gstSummary.NetGstPayable,
                estimatedProfit = netRevenueExTax - netCostExTax,
                ledger
            };
        }

        public async Task<object> GetStockHistoryAsync(int productId, CancellationToken cancellationToken = default)
        {
            var history = await _context.StockLedgers
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .Select(x => new
                {
                    x.LedgerId,
                    x.ProductId,
                    x.ChangeAmount,
                    x.PostChangeBalance,
                    x.ReasonCode,
                    x.ReferenceDocumentId,
                    x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            return new
            {
                productId = productId,
                productName = product?.Name,
                history = history
            };
        }

        public async Task<object> GetExpiryAlertsAsync(int days = 30, CancellationToken cancellationToken = default)
        {
            var horizon = DateTime.UtcNow.AddDays(days);
            var alerts = await _context.StockBatchDetails
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.ExpiryDate <= horizon && x.Quantity > 0)
                .OrderBy(x => x.ExpiryDate)
                .Select(x => new
                {
                    x.BatchNumber,
                    x.ExpiryDate,
                    x.Quantity,
                    ProductName = x.Product!.Name,
                    ProductSku = x.Product.SKU,
                    DaysRemaining = (x.ExpiryDate!.Value - DateTime.UtcNow).Days
                })
                .ToListAsync(cancellationToken);

            return alerts;
        }

        public async Task<object> ScanBatchToOrderAsync(int salesOrderId, string batchOrSerial, CancellationToken cancellationToken = default)
        {
            var batch = await _context.StockBatchDetails
                .Include(x => x.Product)
                .Include(x => x.Bin)
                .FirstOrDefaultAsync(x => x.BatchNumber == batchOrSerial && x.Quantity > 0, cancellationToken)
                ?? throw new InvalidOperationException($"Batch/Serial '{batchOrSerial}' not found or out of stock.");

            var order = await _context.SalesOrders.Include(x => x.Items).Include(x => x.Warehouse).FirstAsync(x => x.Id == salesOrderId, cancellationToken);
            if (order.Status == DocumentStatus.Completed) throw new InvalidOperationException("Order is already invoiced.");

            // Add the item
            var existing = order.Items.FirstOrDefault(x => x.ProductId == batch.ProductId && x.BinId == batch.BinId);
            if (existing != null)
            {
                // In serial tracking, typically quantity is 1. But for batch, we might add more.
                // For "Rapid Scan", we'll just add 1 more.
                existing.Quantity += 1;
            }
            else 
            {
                // Add new line
                var product = batch.Product!;
                order.Items.Add(new SalesOrderItem { 
                    ProductId = batch.ProductId, 
                    BinId = batch.BinId, 
                    Quantity = 1, 
                    UnitPrice = product.SalesPrice, 
                    GstRate = product.GstRate 
                });
            }

            // Recalculate totals
            foreach(var item in order.Items) {
                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, order.Warehouse!.State, order.PlaceOfSupplyState ?? order.Warehouse.State);
                item.TaxableAmount = tax.TaxableAmount;
                item.CgstAmount = tax.CgstAmount;
                item.SgstAmount = tax.SgstAmount;
                item.IgstAmount = tax.IgstAmount;
                item.LineTotal = tax.LineTotal;
            }
            order.Subtotal = order.Items.Sum(x => x.TaxableAmount);
            order.CgstAmount = order.Items.Sum(x => x.CgstAmount);
            order.SgstAmount = order.Items.Sum(x => x.SgstAmount);
            order.IgstAmount = order.Items.Sum(x => x.IgstAmount);
            order.TaxTotal = order.CgstAmount + order.SgstAmount + order.IgstAmount;
            order.GrandTotal = order.Subtotal + order.TaxTotal;

            await _context.SaveChangesAsync(cancellationToken);
            return new { order.Id, order.OrderNumber, ScannedProduct = batch.Product!.Name, order.GrandTotal };
        }
    }
}
