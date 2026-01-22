using FluentValidation;
using Core.DTOs.Movies;

namespace Core.Validators.Movies;

public class CreateMovieDTOValidator : AbstractValidator<CreateMovieDTO>
{
    public  CreateMovieDTOValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("The title is required")
            .MaximumLength(179).WithMessage("The movie title must not exceed 179 characters");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween((ushort)1, (ushort)350).WithMessage("The duration must be between 1 and 350.");
            
        RuleFor(x => x.AgeLimit)
            .GreaterThan((byte)0).WithMessage("The age limit must be greater than 0");
        
        RuleFor(x => x.Genre)
            .IsInEnum().WithMessage("Incorrect genre specified");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("The description must not exceed 1000 characters");
        
        RuleFor(x => x.ReleaseDate)
            .GreaterThanOrEqualTo(new DateOnly(1895, 1, 1)).WithMessage("The released date must be greater than or equal to 1895")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now.AddYears((5))));
        
        RuleFor(x => x.ImdbRating)
            .InclusiveBetween(0, 10).WithMessage("The rating must be between 0 and 10");
        
        RuleFor(x => x.PosterUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.PosterUrl))
            .WithMessage("The poster url must be valid");
        
        RuleFor(x => x.TrailerUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.TrailerUrl))
            .WithMessage("The trailer url must be valid");
        
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("The country must not exceed 100 characters");
        
        RuleFor(x => x.ActorsIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count())
            .WithMessage("The actors ids must be unique");
        
        RuleFor(x => x.DirectorsIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count())
            .WithMessage("The directors ids must be unique");
    }

    private bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}