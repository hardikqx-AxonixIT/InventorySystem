using FluentValidation;
using InventorySystem.Application.Services;
using System.Collections.Generic;

namespace InventorySystem.Application.Validators
{
    public class TransactionLineRequestValidator : AbstractValidator<TransactionLineRequestDto>
    {
        public TransactionLineRequestValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Valid Product is required");
            RuleFor(x => x.BinId).GreaterThan(0).WithMessage("Valid Storage Bin is required");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit Price cannot be negative");
        }
    }

    public class PurchaseOrderCreateValidator : AbstractValidator<PurchaseOrderCreateDto>
    {
        public PurchaseOrderCreateValidator()
        {
            RuleFor(x => x.VendorId).GreaterThan(0).WithMessage("Valid Vendor is required");
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Valid Warehouse is required");
            RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required in the Purchase Order");
            RuleForEach(x => x.Items).SetValidator(new TransactionLineRequestValidator());
        }
    }

    public class SalesOrderCreateValidator : AbstractValidator<SalesOrderCreateDto>
    {
        public SalesOrderCreateValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Valid Customer is required");
            RuleFor(x => x.WarehouseId).GreaterThan(0).WithMessage("Valid Warehouse is required");
            RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required in the Sales Order");
            RuleForEach(x => x.Items).SetValidator(new TransactionLineRequestValidator());
        }
    }

    public class SupplierPaymentCreateValidator : AbstractValidator<SupplierPaymentCreateDto>
    {
        public SupplierPaymentCreateValidator()
        {
            RuleFor(x => x.PurchaseInvoiceId).GreaterThan(0).WithMessage("Valid Purchase Invoice is required");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero");
            RuleFor(x => x.PaymentMode).NotEmpty().WithMessage("Payment mode is required");
        }
    }

    public class StockMovementCreateValidator : AbstractValidator<StockMovementCreateDto>
    {
        public StockMovementCreateValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Valid Product is required");
            RuleFor(x => x.BinId).GreaterThan(0).WithMessage("Valid Bin is required");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero");
        }
    }
}
