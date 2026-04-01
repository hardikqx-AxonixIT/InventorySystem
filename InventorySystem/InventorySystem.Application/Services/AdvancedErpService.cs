using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.Application.Services
{
    public class AdvancedErpService : IAdvancedErpService
    {
        private readonly IApplicationDbContext _context;
        private readonly IStockService _stockService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AdvancedErpService(IApplicationDbContext context, IStockService stockService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _stockService = stockService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<object> GetGstr1Async(GstrExportFilterDto request, CancellationToken cancellationToken = default)
        {
            var (fromDate, toDate) = ResolveDateRange(request.FromDate, request.ToDate);
            var invoices = await _context.SalesInvoices.Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDate).OrderBy(x => x.InvoiceDate).ToListAsync(cancellationToken);
            return new
            {
                ReturnType = "GSTR-1",
                FromDate = fromDate,
                ToDate = toDate,
                Count = invoices.Count,
                Rows = invoices.Select(x => new { x.InvoiceNumber, x.InvoiceDate, x.CustomerId, x.Subtotal, x.CgstAmount, x.SgstAmount, x.IgstAmount, x.GrandTotal })
            };
        }

        public async Task<object> GetGstr3bAsync(GstrExportFilterDto request, CancellationToken cancellationToken = default)
        {
            var (fromDate, toDate) = ResolveDateRange(request.FromDate, request.ToDate);
            var sales = await _context.SalesInvoices.Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDate).ToListAsync(cancellationToken);
            var purchases = await _context.PurchaseInvoices.Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDate).ToListAsync(cancellationToken);
            return new
            {
                ReturnType = "GSTR-3B",
                FromDate = fromDate,
                ToDate = toDate,
                OutwardTaxable = sales.Sum(x => x.Subtotal),
                InputTaxCredit = purchases.Sum(x => x.CgstAmount + x.SgstAmount + x.IgstAmount),
                NetTaxPayable = sales.Sum(x => x.CgstAmount + x.SgstAmount + x.IgstAmount) - purchases.Sum(x => x.CgstAmount + x.SgstAmount + x.IgstAmount)
            };
        }

        public async Task<object> GenerateEInvoiceAsync(EInvoiceGenerateDto request, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.SalesInvoices.FirstAsync(x => x.Id == request.SalesInvoiceId, cancellationToken);
            var existing = await _context.GstEInvoiceRecords.FirstOrDefaultAsync(x => x.SalesInvoiceId == invoice.Id, cancellationToken);
            if (existing != null) return new { existing.Id, existing.SalesInvoiceId, existing.Irn, existing.AckNo, existing.AckDate };

            var payload = $"{invoice.InvoiceNumber}|{invoice.InvoiceDate:yyyyMMdd}|{invoice.GrandTotal:0.00}";
            var record = new GstEInvoiceRecord
            {
                SalesInvoiceId = invoice.Id,
                Irn = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))),
                AckNo = $"ACK{DateTime.UtcNow:yyyyMMddHHmmss}",
                AckDate = DateTime.UtcNow,
                SignedInvoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
            };
            _context.GstEInvoiceRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken);
            return new { record.Id, record.SalesInvoiceId, record.Irn, record.AckNo, record.AckDate };
        }

        public async Task<object> GenerateEWayBillAsync(EWayBillGenerateDto request, CancellationToken cancellationToken = default)
        {
            var existing = await _context.GstEWayBillRecords.FirstOrDefaultAsync(x => x.SalesInvoiceId == request.SalesInvoiceId, cancellationToken);
            if (existing != null) return new { existing.Id, existing.EWayBillNumber, existing.ValidUntil };
            var record = new GstEWayBillRecord
            {
                SalesInvoiceId = request.SalesInvoiceId,
                EWayBillNumber = $"{DateTime.UtcNow:yyyyMMdd}{request.SalesInvoiceId:000000}",
                VehicleNumber = request.VehicleNumber,
                DistanceKm = request.DistanceKm,
                ValidUntil = DateTime.UtcNow.AddHours(Math.Max(24, (int)Math.Ceiling(request.DistanceKm / 100m) * 24))
            };
            _context.GstEWayBillRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken);
            return new { record.Id, record.EWayBillNumber, record.ValidUntil };
        }

        public async Task<object> GetAccountingLedgersAsync(CancellationToken cancellationToken = default)
        {
            var ledgers = await _context.AccountLedgers.OrderBy(x => x.GroupType).ThenBy(x => x.Name).ToListAsync(cancellationToken);
            return ledgers.Select(x => new { x.Id, x.Code, x.Name, x.GroupType, x.IsSystem, x.IsActive });
        }

        public async Task<object> PostJournalVoucherAsync(JournalVoucherCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.Lines.Count < 2) throw new InvalidOperationException("Journal voucher requires at least two lines.");
            var debit = request.Lines.Sum(x => x.Debit);
            var credit = request.Lines.Sum(x => x.Credit);
            if (decimal.Round(debit, 2) != decimal.Round(credit, 2)) throw new InvalidOperationException("Debit and credit totals must match.");
            var voucher = new JournalVoucher { VoucherNumber = $"JV-{DateTime.UtcNow:yyyyMMddHHmmss}", VoucherDate = request.VoucherDate ?? DateTime.UtcNow, Narration = request.Narration, SourceModule = request.SourceModule, SourceDocumentNo = request.SourceDocumentNo };
            foreach (var line in request.Lines) voucher.Lines.Add(new JournalVoucherLine { LedgerId = line.LedgerId, Debit = line.Debit, Credit = line.Credit, Remarks = line.Remarks });
            _context.JournalVouchers.Add(voucher);
            await _context.SaveChangesAsync(cancellationToken);
            return new { voucher.Id, voucher.VoucherNumber, voucher.VoucherDate, TotalDebit = debit, TotalCredit = credit };
        }

        public async Task<object> GetLedgerDrilldownAsync(int ledgerId, DateRangeFilterDto request, CancellationToken cancellationToken = default)
        {
            var ledger = await _context.AccountLedgers.FirstAsync(x => x.Id == ledgerId, cancellationToken);
            var (fromDate, toDate) = ResolveDateRange(request.FromDate, request.ToDate);
            var lines = await _context.JournalVoucherLines.Include(x => x.JournalVoucher)
                .Where(x => x.LedgerId == ledgerId && x.JournalVoucher != null && x.JournalVoucher.VoucherDate >= fromDate && x.JournalVoucher.VoucherDate <= toDate)
                .OrderBy(x => x.JournalVoucher!.VoucherDate).ToListAsync(cancellationToken);
            return new { ledger.Id, ledger.Code, ledger.Name, FromDate = fromDate, ToDate = toDate, DebitTotal = lines.Sum(x => x.Debit), CreditTotal = lines.Sum(x => x.Credit), Entries = lines.Select(x => new { Date = x.JournalVoucher!.VoucherDate, x.JournalVoucher.VoucherNumber, x.Debit, x.Credit, x.Remarks }) };
        }

        public async Task<object> GetProfitAndLossAsync(DateRangeFilterDto request, CancellationToken cancellationToken = default)
        {
            var (fromDate, toDate) = ResolveDateRange(request.FromDate, request.ToDate);
            var lines = await _context.JournalVoucherLines.Include(x => x.JournalVoucher).Include(x => x.Ledger)
                .Where(x => x.JournalVoucher != null && x.JournalVoucher.VoucherDate >= fromDate && x.JournalVoucher.VoucherDate <= toDate && x.Ledger != null && (x.Ledger.GroupType == "Income" || x.Ledger.GroupType == "Expense"))
                .ToListAsync(cancellationToken);
            var income = lines.Where(x => x.Ledger!.GroupType == "Income").Sum(x => x.Credit - x.Debit);
            var expense = lines.Where(x => x.Ledger!.GroupType == "Expense").Sum(x => x.Debit - x.Credit);
            return new { FromDate = fromDate, ToDate = toDate, TotalIncome = income, TotalExpense = expense, NetProfit = income - expense };
        }

        public async Task<object> GetBalanceSheetAsync(DateTime? asOfDate, CancellationToken cancellationToken = default)
        {
            var date = asOfDate ?? DateTime.UtcNow.Date;
            var lines = await _context.JournalVoucherLines.Include(x => x.JournalVoucher).Include(x => x.Ledger)
                .Where(x => x.JournalVoucher != null && x.JournalVoucher.VoucherDate <= date && x.Ledger != null && (x.Ledger.GroupType == "Asset" || x.Ledger.GroupType == "Liability"))
                .ToListAsync(cancellationToken);
            var assets = lines.Where(x => x.Ledger!.GroupType == "Asset").Sum(x => x.Debit - x.Credit);
            var liabilities = lines.Where(x => x.Ledger!.GroupType == "Liability").Sum(x => x.Credit - x.Debit);
            return new { AsOfDate = date, TotalAssets = assets, TotalLiabilities = liabilities };
        }

        public async Task<object> CreateQuotationAsync(QuotationCreateDto request, CancellationToken cancellationToken = default)
        {
            var warehouse = await _context.Warehouses.FirstAsync(w => w.Id == request.WarehouseId, cancellationToken);
            var quote = new SalesQuotation { QuotationNumber = $"QT-{DateTime.UtcNow:yyyyMMddHHmmss}", CustomerId = request.CustomerId, WarehouseId = request.WarehouseId, ValidUntil = request.ValidUntil, Status = DocumentStatus.Open };
            foreach (var item in request.Items)
            {
                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, warehouse.State, request.PlaceOfSupplyState ?? warehouse.State);
                quote.Items.Add(new SalesQuotationItem { ProductId = item.ProductId, BinId = item.BinId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, GstRate = item.GstRate, TaxableAmount = tax.TaxableAmount, CgstAmount = tax.CgstAmount, SgstAmount = tax.SgstAmount, IgstAmount = tax.IgstAmount, LineTotal = tax.LineTotal });
            }
            quote.Subtotal = quote.Items.Sum(x => x.TaxableAmount);
            quote.CgstAmount = quote.Items.Sum(x => x.CgstAmount);
            quote.SgstAmount = quote.Items.Sum(x => x.SgstAmount);
            quote.IgstAmount = quote.Items.Sum(x => x.IgstAmount);
            quote.TaxTotal = quote.CgstAmount + quote.SgstAmount + quote.IgstAmount;
            quote.GrandTotal = quote.Subtotal + quote.TaxTotal;
            _context.SalesQuotations.Add(quote);
            await _context.SaveChangesAsync(cancellationToken);
            return new { quote.Id, quote.QuotationNumber, quote.GrandTotal, quote.Status };
        }

        public async Task<object> UpdateQuotationAsync(int quotationId, QuotationUpdateDto request, CancellationToken cancellationToken = default)
        {
            var quote = await _context.SalesQuotations.Include(x => x.Warehouse).Include(x => x.Items).FirstAsync(x => x.Id == quotationId, cancellationToken);
            if (quote.Status == DocumentStatus.Completed || quote.Status == DocumentStatus.Cancelled) throw new InvalidOperationException("Only open quotations can be updated.");

            var warehouse = await _context.Warehouses.FirstAsync(w => w.Id == request.WarehouseId, cancellationToken);
            _context.SalesQuotationItems.RemoveRange(quote.Items);
            quote.Items.Clear();

            quote.CustomerId = request.CustomerId;
            quote.WarehouseId = request.WarehouseId;
            quote.ValidUntil = request.ValidUntil;
            foreach (var item in request.Items)
            {
                var tax = CalculateTax(item.Quantity, item.UnitPrice, item.GstRate, warehouse.State, request.PlaceOfSupplyState ?? warehouse.State);
                quote.Items.Add(new SalesQuotationItem { ProductId = item.ProductId, BinId = item.BinId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, GstRate = item.GstRate, TaxableAmount = tax.TaxableAmount, CgstAmount = tax.CgstAmount, SgstAmount = tax.SgstAmount, IgstAmount = tax.IgstAmount, LineTotal = tax.LineTotal });
            }

            quote.Subtotal = quote.Items.Sum(x => x.TaxableAmount);
            quote.CgstAmount = quote.Items.Sum(x => x.CgstAmount);
            quote.SgstAmount = quote.Items.Sum(x => x.SgstAmount);
            quote.IgstAmount = quote.Items.Sum(x => x.IgstAmount);
            quote.TaxTotal = quote.CgstAmount + quote.SgstAmount + quote.IgstAmount;
            quote.GrandTotal = quote.Subtotal + quote.TaxTotal;

            await _context.SaveChangesAsync(cancellationToken);
            return new { quote.Id, quote.QuotationNumber, quote.GrandTotal, quote.Status };
        }

        public async Task<object> CancelQuotationAsync(int quotationId, CancellationToken cancellationToken = default)
        {
            var quote = await _context.SalesQuotations.FirstAsync(x => x.Id == quotationId, cancellationToken);
            if (quote.Status == DocumentStatus.Completed) throw new InvalidOperationException("Converted quotation cannot be cancelled.");
            quote.Status = DocumentStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
            return new { quote.Id, quote.QuotationNumber, quote.Status };
        }

        public async Task<object> ConvertQuotationToOrderAsync(int quotationId, CancellationToken cancellationToken = default)
        {
            var quotation = await _context.SalesQuotations.Include(x => x.Warehouse).Include(x => x.Items).FirstAsync(x => x.Id == quotationId, cancellationToken);
            if (quotation.Status == DocumentStatus.Completed) throw new InvalidOperationException("Quotation already converted.");
            var order = new SalesOrder { OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}", CustomerId = quotation.CustomerId, WarehouseId = quotation.WarehouseId, PlaceOfSupplyState = quotation.Warehouse?.State };
            foreach (var qLine in quotation.Items)
            {
                var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.ProductId == qLine.ProductId && s.BinId == qLine.BinId, cancellationToken);
                if (stock == null || stock.QuantityOnHand - stock.ReservedQuantity < qLine.Quantity) throw new InvalidOperationException("Insufficient stock to convert quotation.");
                stock.ReservedQuantity += qLine.Quantity;
                order.Items.Add(new SalesOrderItem { ProductId = qLine.ProductId, BinId = qLine.BinId, Quantity = qLine.Quantity, UnitPrice = qLine.UnitPrice, GstRate = qLine.GstRate, TaxableAmount = qLine.TaxableAmount, CgstAmount = qLine.CgstAmount, SgstAmount = qLine.SgstAmount, IgstAmount = qLine.IgstAmount, LineTotal = qLine.LineTotal });
            }
            order.Subtotal = order.Items.Sum(x => x.TaxableAmount);
            order.CgstAmount = order.Items.Sum(x => x.CgstAmount);
            order.SgstAmount = order.Items.Sum(x => x.SgstAmount);
            order.IgstAmount = order.Items.Sum(x => x.IgstAmount);
            order.TaxTotal = order.CgstAmount + order.SgstAmount + order.IgstAmount;
            order.GrandTotal = order.Subtotal + order.TaxTotal;
            quotation.Status = DocumentStatus.Completed;
            _context.SalesOrders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);
            return new { quotation.Id, quotation.QuotationNumber, SalesOrderId = order.Id, order.OrderNumber };
        }

        public async Task<object> CreateDeliveryChallanAsync(DeliveryChallanCreateDto request, CancellationToken cancellationToken = default)
        {
            var order = await _context.SalesOrders.Include(x => x.Items).FirstAsync(x => x.Id == request.SalesOrderId, cancellationToken);
            var challan = new DeliveryChallan { SalesOrderId = order.Id, ChallanNumber = $"DC-{DateTime.UtcNow:yyyyMMddHHmmss}", Notes = request.Notes, Status = DocumentStatus.Completed };
            foreach (var item in order.Items) challan.Items.Add(new DeliveryChallanItem { SalesOrderItemId = item.Id, ProductId = item.ProductId, BinId = item.BinId, Quantity = item.Quantity });
            _context.DeliveryChallans.Add(challan);
            await _context.SaveChangesAsync(cancellationToken);
            return new { challan.Id, challan.ChallanNumber, challan.SalesOrderId, challan.Status };
        }

        public async Task<object> CreateSalesReturnAsync(SalesReturnCreateDto request, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.SalesInvoices.Include(x => x.Items).FirstAsync(x => x.Id == request.SalesInvoiceId, cancellationToken);
            var ret = new SalesReturn { SalesInvoiceId = invoice.Id, ReturnNumber = $"SR-{DateTime.UtcNow:yyyyMMddHHmmss}", Reason = request.Reason };
            foreach (var line in request.Items)
            {
                var invoiceLine = invoice.Items.FirstOrDefault(x => x.Id == line.SalesInvoiceItemId) ?? throw new InvalidOperationException("Invalid invoice line.");
                if (line.Quantity <= 0 || line.Quantity > invoiceLine.Quantity) throw new InvalidOperationException("Invalid return quantity.");
                var factor = line.Quantity / invoiceLine.Quantity;
                ret.Items.Add(new SalesReturnItem { SalesInvoiceItemId = invoiceLine.Id, ProductId = invoiceLine.ProductId, BinId = line.BinId, Quantity = line.Quantity, UnitPrice = invoiceLine.UnitPrice, GstRate = invoiceLine.GstRate, TaxableAmount = decimal.Round(invoiceLine.TaxableAmount * factor, 2), CgstAmount = decimal.Round(invoiceLine.CgstAmount * factor, 2), SgstAmount = decimal.Round(invoiceLine.SgstAmount * factor, 2), IgstAmount = decimal.Round(invoiceLine.IgstAmount * factor, 2), LineTotal = decimal.Round(invoiceLine.LineTotal * factor, 2) });
                await _stockService.PerformStockMovementAsync(invoiceLine.ProductId, line.BinId, line.Quantity, "SALES_RETURN", ret.ReturnNumber, cancellationToken);
            }
            ret.Subtotal = ret.Items.Sum(x => x.TaxableAmount);
            ret.CgstAmount = ret.Items.Sum(x => x.CgstAmount);
            ret.SgstAmount = ret.Items.Sum(x => x.SgstAmount);
            ret.IgstAmount = ret.Items.Sum(x => x.IgstAmount);
            ret.TaxTotal = ret.CgstAmount + ret.SgstAmount + ret.IgstAmount;
            ret.GrandTotal = ret.Subtotal + ret.TaxTotal;
            _context.SalesReturns.Add(ret);
            await _context.SaveChangesAsync(cancellationToken);
            return new { ret.Id, ret.ReturnNumber, ret.GrandTotal };
        }

        public async Task<object> CreateTransferRequestAsync(TransferRequestCreateDto request, CancellationToken cancellationToken = default)
        {
            var tr = new WarehouseTransferRequest { RequestNumber = $"TRQ-{DateTime.UtcNow:yyyyMMddHHmmss}", ProductId = request.ProductId, FromBinId = request.FromBinId, ToBinId = request.ToBinId, Quantity = request.Quantity, RequestedBy = request.RequestedBy, Status = DocumentStatus.Open };
            _context.WarehouseTransferRequests.Add(tr);
            await _context.SaveChangesAsync(cancellationToken);
            return new { tr.Id, tr.RequestNumber, tr.Status };
        }

        public async Task<object> ApproveTransferRequestAsync(TransferRequestApprovalDto request, CancellationToken cancellationToken = default)
        {
            var tr = await _context.WarehouseTransferRequests.FirstAsync(x => x.Id == request.RequestId, cancellationToken);
            if (tr.Status != DocumentStatus.Open) throw new InvalidOperationException("Transfer request already processed.");
            tr.ApprovedBy = request.ApprovedBy;
            tr.ApprovalNote = request.ApprovalNote;
            if (request.Approve)
            {
                await _stockService.PerformStockMovementAsync(tr.ProductId, tr.FromBinId, -tr.Quantity, "TRANSFER_APPROVED_OUT", tr.RequestNumber, cancellationToken);
                await _stockService.PerformStockMovementAsync(tr.ProductId, tr.ToBinId, tr.Quantity, "TRANSFER_APPROVED_IN", tr.RequestNumber, cancellationToken);
                tr.Status = DocumentStatus.Completed;
            }
            else tr.Status = DocumentStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
            return new { tr.Id, tr.RequestNumber, tr.Status };
        }

        public async Task<object> CreatePickListAsync(PickListCreateDto request, CancellationToken cancellationToken = default)
        {
            var order = await _context.SalesOrders.Include(x => x.Items).FirstAsync(x => x.Id == request.SalesOrderId, cancellationToken);
            var pick = new PickList { PickNumber = $"PK-{DateTime.UtcNow:yyyyMMddHHmmss}", SalesOrderId = order.Id, Status = DocumentStatus.Open };
            foreach (var line in order.Items) pick.Items.Add(new PickListItem { SalesOrderItemId = line.Id, ProductId = line.ProductId, BinId = line.BinId, Quantity = line.Quantity, PickedQuantity = 0 });
            _context.PickLists.Add(pick);
            await _context.SaveChangesAsync(cancellationToken);
            return new { pick.Id, pick.PickNumber, pick.Status };
        }

        public async Task<object> ScanPickItemAsync(PickScanDto request, CancellationToken cancellationToken = default)
        {
            var pick = await _context.PickLists.Include(x => x.Items).FirstAsync(x => x.Id == request.PickListId, cancellationToken);
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Barcode == request.Barcode, cancellationToken) ?? throw new InvalidOperationException("Barcode not mapped.");
            var line = pick.Items.FirstOrDefault(x => x.ProductId == product.Id) ?? throw new InvalidOperationException("Scanned product not in pick list.");
            var remaining = line.Quantity - line.PickedQuantity;
            if (remaining <= 0) throw new InvalidOperationException("Already fully picked.");
            line.PickedQuantity += Math.Min(request.Quantity <= 0 ? 1 : request.Quantity, remaining);
            if (pick.Items.All(x => x.PickedQuantity >= x.Quantity)) pick.Status = DocumentStatus.PartiallyProcessed;
            await _context.SaveChangesAsync(cancellationToken);
            return new { pick.Id, pick.PickNumber, pick.Status, Product = product.Name, line.Quantity, line.PickedQuantity };
        }

        public async Task<object> MarkPickListPackedAsync(int pickListId, CancellationToken cancellationToken = default)
        {
            var pick = await _context.PickLists.Include(x => x.Items).FirstAsync(x => x.Id == pickListId, cancellationToken);
            if (pick.Items.Any(x => x.PickedQuantity < x.Quantity)) throw new InvalidOperationException("Cannot pack until all items are picked.");
            pick.Status = DocumentStatus.Completed;
            await _context.SaveChangesAsync(cancellationToken);
            return new { pick.Id, pick.PickNumber, pick.Status };
        }

        public async Task<object> CreateBomTemplateAsync(BomTemplateCreateDto request, CancellationToken cancellationToken = default)
        {
            if (request.Items.Count == 0) throw new InvalidOperationException("BOM requires at least one component.");
            var bom = new BomTemplate { BomCode = string.IsNullOrWhiteSpace(request.BomCode) ? $"BOM-{DateTime.UtcNow:yyyyMMddHHmmss}" : request.BomCode.Trim(), Name = request.Name.Trim(), FinishedProductId = request.FinishedProductId, StandardOutputQty = request.StandardOutputQty > 0 ? request.StandardOutputQty : 1 };
            foreach (var i in request.Items) bom.Items.Add(new BomTemplateItem { ComponentProductId = i.ComponentProductId, QuantityPerOutput = i.QuantityPerOutput });
            _context.BomTemplates.Add(bom);
            await _context.SaveChangesAsync(cancellationToken);
            return new { bom.Id, bom.BomCode, bom.Name, bom.FinishedProductId };
        }

        public async Task<object> UpdateBomTemplateAsync(int bomTemplateId, BomTemplateUpdateDto request, CancellationToken cancellationToken = default)
        {
            var bom = await _context.BomTemplates.Include(x => x.Items).FirstAsync(x => x.Id == bomTemplateId, cancellationToken);
            bom.Name = request.Name.Trim();
            bom.StandardOutputQty = request.StandardOutputQty > 0 ? request.StandardOutputQty : 1;
            bom.IsActive = request.IsActive;

            _context.BomTemplateItems.RemoveRange(bom.Items);
            bom.Items.Clear();
            foreach (var i in request.Items)
            {
                bom.Items.Add(new BomTemplateItem { ComponentProductId = i.ComponentProductId, QuantityPerOutput = i.QuantityPerOutput });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new { bom.Id, bom.BomCode, bom.Name, bom.IsActive };
        }

        public async Task<object> CreateWorkOrderAsync(WorkOrderCreateDto request, CancellationToken cancellationToken = default)
        {
            var wo = new ProductionWorkOrder { WorkOrderNumber = $"WO-{DateTime.UtcNow:yyyyMMddHHmmss}", BomTemplateId = request.BomTemplateId, PlannedDate = request.PlannedDate, PlannedOutputQty = request.PlannedOutputQty, InputBinId = request.InputBinId, OutputBinId = request.OutputBinId, Status = DocumentStatus.Open };
            _context.ProductionWorkOrders.Add(wo);
            await _context.SaveChangesAsync(cancellationToken);
            return new { wo.Id, wo.WorkOrderNumber, wo.Status };
        }

        public async Task<object> CancelWorkOrderAsync(int workOrderId, CancellationToken cancellationToken = default)
        {
            var wo = await _context.ProductionWorkOrders.FirstAsync(x => x.Id == workOrderId, cancellationToken);
            if (wo.Status == DocumentStatus.Completed) throw new InvalidOperationException("Completed work order cannot be cancelled.");
            wo.Status = DocumentStatus.Cancelled;
            await _context.SaveChangesAsync(cancellationToken);
            return new { wo.Id, wo.WorkOrderNumber, wo.Status };
        }

        public async Task<object> ReleaseWorkOrderAsync(int workOrderId, CancellationToken cancellationToken = default)
        {
            var wo = await _context.ProductionWorkOrders.Include(x => x.BomTemplate).ThenInclude(x => x!.Items).FirstAsync(x => x.Id == workOrderId, cancellationToken);
            if (wo.Status != DocumentStatus.Open) throw new InvalidOperationException("Work order already released.");
            var bom = wo.BomTemplate ?? throw new InvalidOperationException("BOM not found.");
            var factor = wo.PlannedOutputQty / (bom.StandardOutputQty <= 0 ? 1 : bom.StandardOutputQty);
            foreach (var comp in bom.Items) await _stockService.PerformStockMovementAsync(comp.ComponentProductId, wo.InputBinId, decimal.Round(comp.QuantityPerOutput * factor, 4) * -1, "MFG_CONSUME", wo.WorkOrderNumber, cancellationToken);
            await _stockService.PerformStockMovementAsync(bom.FinishedProductId, wo.OutputBinId, wo.PlannedOutputQty, "MFG_OUTPUT", wo.WorkOrderNumber, cancellationToken);
            wo.Status = DocumentStatus.Completed;
            await _context.SaveChangesAsync(cancellationToken);
            return new { wo.Id, wo.WorkOrderNumber, wo.Status };
        }

        public async Task<object> GetRolePermissionMatrixAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _context.RolePermissions.OrderBy(x => x.RoleName).ThenBy(x => x.ModuleKey).ToListAsync(cancellationToken);
            return rows.Select(x => new { x.Id, x.RoleName, x.ModuleKey, x.CanView, x.CanCreate, x.CanUpdate, x.CanDelete, x.CanApprove, x.CanExport });
        }

        public async Task<object> UpsertRolePermissionAsync(PermissionUpsertDto request, CancellationToken cancellationToken = default)
        {
            var roleName = request.RoleName.Trim();
            var module = request.ModuleKey.Trim().ToLowerInvariant();
            var row = await _context.RolePermissions.FirstOrDefaultAsync(x => x.RoleName == roleName && x.ModuleKey == module, cancellationToken);
            if (row == null) { row = new RolePermission { RoleName = roleName, ModuleKey = module }; _context.RolePermissions.Add(row); }
            row.CanView = request.CanView; row.CanCreate = request.CanCreate; row.CanUpdate = request.CanUpdate; row.CanDelete = request.CanDelete; row.CanApprove = request.CanApprove; row.CanExport = request.CanExport;
            await _context.SaveChangesAsync(cancellationToken);
            return new { row.Id, row.RoleName, row.ModuleKey, row.CanView, row.CanCreate, row.CanUpdate, row.CanDelete, row.CanApprove, row.CanExport };
        }

        public async Task<object> GetAuditLogsAsync(AuditLogFilterDto request, CancellationToken cancellationToken = default)
        {
            var q = _context.AuditLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.EntityName)) q = q.Where(x => x.EntityName.Contains(request.EntityName.Trim()));
            if (!string.IsNullOrWhiteSpace(request.Action)) q = q.Where(x => x.Action == request.Action.Trim());
            var page = request.Page < 1 ? 1 : request.Page;
            var size = request.PageSize < 1 ? 50 : Math.Min(request.PageSize, 200);
            var total = await q.CountAsync(cancellationToken);
            var rows = await q.OrderByDescending(x => x.Timestamp).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
            return new { Page = page, PageSize = size, Total = total, Records = rows.Select(x => new { x.Id, x.Timestamp, x.EntityName, x.EntityId, x.Action, x.UserId, x.OldValues, x.NewValues }) };
        }

        public async Task<object> SendWhatsAppInvoiceAsync(WhatsAppInvoiceDto request, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.SalesInvoices.FirstAsync(x => x.Id == request.SalesInvoiceId, cancellationToken);
            var token = _configuration["Integrations:WhatsApp:AccessToken"];
            var phoneId = _configuration["Integrations:WhatsApp:PhoneNumberId"];
            var msg = $"Invoice {invoice.InvoiceNumber} amount INR {invoice.GrandTotal:0.00}";
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(phoneId))
                return new { Success = false, ProviderConfigured = false, PreviewUrl = $"https://wa.me/{request.PhoneNumber}?text={Uri.EscapeDataString(msg)}", Note = "Configure WhatsApp API settings." };
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await client.PostAsJsonAsync($"https://graph.facebook.com/v19.0/{phoneId}/messages", new { messaging_product = "whatsapp", to = request.PhoneNumber, type = "text", text = new { body = msg } }, cancellationToken);
            return new { Success = resp.IsSuccessStatusCode, ProviderConfigured = true, StatusCode = (int)resp.StatusCode, Response = await resp.Content.ReadAsStringAsync(cancellationToken) };
        }

        public async Task<object> CreateRazorpayOrderAsync(RazorpayOrderCreateDto request, CancellationToken cancellationToken = default)
        {
            var keyId = _configuration["Integrations:Razorpay:KeyId"];
            var secret = _configuration["Integrations:Razorpay:KeySecret"];
            if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(secret)) return new { Success = false, ProviderConfigured = false, Note = "Configure Razorpay credentials." };
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{keyId}:{secret}")));
            var resp = await client.PostAsJsonAsync("https://api.razorpay.com/v1/orders", new { amount = (int)Math.Round(request.Amount * 100m, 0), currency = request.Currency, receipt = string.IsNullOrWhiteSpace(request.Receipt) ? $"rcpt-{DateTime.UtcNow:yyyyMMddHHmmss}" : request.Receipt, payment_capture = 1 }, cancellationToken);
            return new { Success = resp.IsSuccessStatusCode, ProviderConfigured = true, StatusCode = (int)resp.StatusCode, Response = await resp.Content.ReadAsStringAsync(cancellationToken) };
        }

        public async Task<object> VerifyRazorpayCallbackAsync(RazorpayCallbackDto request, CancellationToken cancellationToken = default)
        {
            var secret = _configuration["Integrations:Razorpay:KeySecret"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(secret)) throw new InvalidOperationException("Razorpay secret is not configured.");
            var payload = $"{request.RazorpayOrderId}|{request.RazorpayPaymentId}";
            var sig = ComputeHmacSha256(payload, secret);
            var verified = string.Equals(sig, request.RazorpaySignature, StringComparison.OrdinalIgnoreCase);
            _context.PaymentGatewayCallbackLogs.Add(new PaymentGatewayCallbackLog { GatewayName = "Razorpay", ExternalOrderId = request.RazorpayOrderId, ExternalPaymentId = request.RazorpayPaymentId, SignatureVerified = verified, RawPayload = request.RawPayload });
            await _context.SaveChangesAsync(cancellationToken);
            return new { Verified = verified, request.RazorpayOrderId, request.RazorpayPaymentId };
        }

        public async Task<object> GetAdvancedSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var quotations = await _context.SalesQuotations.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var challans = await _context.DeliveryChallans.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var returns = await _context.SalesReturns.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var transferRequests = await _context.WarehouseTransferRequests.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var pickLists = await _context.PickLists.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var bomTemplates = await _context.BomTemplates.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var workOrders = await _context.ProductionWorkOrders.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var permissions = await _context.RolePermissions.OrderBy(x => x.RoleName).ThenBy(x => x.ModuleKey).ToListAsync(cancellationToken);
            var eInvoices = await _context.GstEInvoiceRecords.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            var eWayBills = await _context.GstEWayBillRecords.OrderByDescending(x => x.Id).Take(20).ToListAsync(cancellationToken);
            return new { quotations, deliveryChallans = challans, salesReturns = returns, transferRequests, pickLists, bomTemplates, workOrders, permissions, eInvoices, eWayBills };
        }

        private static (DateTime FromDate, DateTime ToDate) ResolveDateRange(DateTime? fromDate, DateTime? toDate)
        {
            var to = (toDate ?? DateTime.UtcNow.Date).Date.AddDays(1).AddTicks(-1);
            var from = (fromDate ?? to.AddDays(-30)).Date;
            return (from, to);
        }

        private static string ComputeHmacSha256(string text, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private static TaxBreakdownDto CalculateTax(decimal quantity, decimal unitPrice, decimal gstRate, string? originState, string? destinationState)
        {
            var taxable = Math.Round(quantity * unitPrice, 2);
            var totalTax = Math.Round(taxable * gstRate / 100m, 2);
            var intra = string.Equals(originState?.Trim(), destinationState?.Trim(), StringComparison.OrdinalIgnoreCase);
            if (intra)
            {
                var half = Math.Round(totalTax / 2m, 2);
                return new TaxBreakdownDto { TaxableAmount = taxable, CgstAmount = half, SgstAmount = totalTax - half, IgstAmount = 0, LineTotal = taxable + totalTax };
            }
            return new TaxBreakdownDto { TaxableAmount = taxable, CgstAmount = 0, SgstAmount = 0, IgstAmount = totalTax, LineTotal = taxable + totalTax };
        }
    }
}
