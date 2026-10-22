using CountrySecure.Application.DTOs.Order;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrderType).IsInEnum();
    }
}

public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500).When(x => x.Description != null);
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(200).When(x => x.SupplierName != null);
        RuleFor(x => x.OrderType).IsInEnum().When(x => x.OrderType.HasValue);
    }
}
