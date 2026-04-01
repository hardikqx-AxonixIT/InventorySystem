using FluentValidation;
using InventorySystem.Application.Services;

namespace InventorySystem.Application.Validators
{
    public class AdjustmentRequestValidator : AbstractValidator<AdjustmentRequestDto>
    {
        public AdjustmentRequestValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product ID must be greater than zero");
            RuleFor(x => x.Amount).NotEqual(0).WithMessage("Adjustment quantity cannot be zero");
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(200).WithMessage("A reason (max 200 chars) is required for adjustment");
        }
    }
}
