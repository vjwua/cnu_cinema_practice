using Core.DTOs.Sessions;
using FluentValidation;

namespace Core.Validators.Sessions;

public class CreateSessionDtoValidator : AbstractValidator<CreateSessionDTO>
{
    public CreateSessionDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.HallId)
            .GreaterThan(0)
            .WithMessage("HallId must be greater than 0.");

        RuleFor(x => x.StartTime)
            .Must(dt => dt > DateTime.UtcNow)
            .WithMessage("Start time must be in the future (UTC).");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Base price must be non-negative.");

        RuleFor(x => x.MovieFormat)
            .IsInEnum()
            .WithMessage("Invalid movie format.");
    }
}