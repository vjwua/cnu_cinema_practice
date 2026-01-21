using Core.DTOs.Sessions;
using FluentValidation;

namespace Core.Validators.Sessions;

public class UpdateSessionDtoValidator : AbstractValidator<UpdateSessionDTO>
{
    public UpdateSessionDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.")
            .When(x => x.MovieId.HasValue);

        RuleFor(x => x.HallId)
            .GreaterThan(0)
            .WithMessage("HallId must be greater than 0.")
            .When(x => x.HallId.HasValue);

        RuleFor(x => x.StartTime)
            .Must(dt => !dt.HasValue || dt.Value > DateTime.UtcNow)
            .WithMessage("Start time must be in the future (UTC).");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Base price must be non-negative.")
            .When(x => x.BasePrice.HasValue);

        RuleFor(x => x.MovieFormat)
            .IsInEnum()
            .WithMessage("Invalid movie format.")
            .When(x => x.MovieFormat.HasValue);

        RuleFor(x => x)
            .Must(dto => dto.MovieId.HasValue || dto.HallId.HasValue || dto.StartTime.HasValue || dto.BasePrice.HasValue || dto.MovieFormat.HasValue)
            .WithMessage("At least one field must be provided to update.");
    }
}