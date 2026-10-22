using CountrySecure.Application.DTOs.Amenities;
using FluentValidation;

namespace CountrySecure.Application.Validators;

public class AmenityCreateValidator : AbstractValidator<AmenityCreateDto>
{
    public AmenityCreateValidator()
    {
        RuleFor(x => x.AmenityName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Schedules).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 1000).WithMessage("La capacidad debe estar entre 1 y 1000.");
    }
}

public class AmenityUpdateValidator : AbstractValidator<AmenityUpdateDto>
{
    public AmenityUpdateValidator()
    {
        RuleFor(x => x.AmenityName).NotEmpty().MaximumLength(100).When(x => x.AmenityName != null);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500).When(x => x.Description != null);
        RuleFor(x => x.Schedules).NotEmpty().MaximumLength(200).When(x => x.Schedules != null);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 1000).When(x => x.Capacity.HasValue);
    }
}
