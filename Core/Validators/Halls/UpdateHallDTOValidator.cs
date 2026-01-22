using Core.DTOs.Halls;
using FluentValidation;

namespace Core.Validators.Hall;

public class UpdateHallDTOValidator : AbstractValidator<UpdateHallDTO>
{
    public UpdateHallDTOValidator()
    {
        RuleFor(h => h.Id)
            .NotEmpty()
            .WithMessage("Hall ID must not be empty");
        RuleFor(h => h)
            .Must(hasAtLeastOneParameter)
            .WithMessage("You have to update at least one parameter (name | layout)");
        RuleFor(h => h)
            .Must(hasProperSeatLayout)
            .WithMessage("Hall must have between 1 and 50 Rows and Columns");
        RuleFor(h => h)
            .Must(hasProperName)
            .WithMessage("Hall name must be less than 101 characters");
    }

    private bool hasAtLeastOneParameter(UpdateHallDTO dto)
    {
        if (dto.Name == null && dto.SeatLayout == null) return false;
        return true;
    }

    private bool hasProperSeatLayout(UpdateHallDTO dto)
    {
        if (dto.SeatLayout == null) return true;
        int rows = dto.SeatLayout.GetLength(0);
        int columns = dto.SeatLayout.GetLength(1);
        if ((1 <= rows && rows <= 50)
            && (1 <= columns && columns <= 50))
        {
            return true;
        }

        return false;
    }

    private bool hasProperName(UpdateHallDTO dto)
    {
        if (dto.Name == null) return true;
        if (dto.Name.Length <= 100) return true;
        return false;
    }
}