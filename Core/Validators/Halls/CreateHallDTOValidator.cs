using FluentValidation;
using Core.DTOs.Halls;

namespace Core.Validators.Hall;

public class CreateHallDTOValidator : AbstractValidator<CreateHallDTO>
{
    public CreateHallDTOValidator()
    {
        RuleFor(h => h.Name)
            .NotEmpty().WithMessage("Hall name is required")
            .MaximumLength(100).WithMessage("Hall name cannot exceed 100 characters");
        RuleFor(h => h.Columns)
            .InclusiveBetween((byte) 1, (byte) 50)
            .WithMessage("Hall must contain between 1 and 50 columns");
        RuleFor(h => h.Rows)
            .InclusiveBetween((byte) 1, (byte) 50)
            .WithMessage("Hall must contain between 1 and 50 rows");
        RuleFor(h => h)
            .Must(DimensionsAlign)
            .WithMessage("Layout dimensions must align with Rows and Columns values");
    }

    private bool DimensionsAlign(CreateHallDTO dto)
    {
        if (dto.SeatLayout.GetLength(0) == dto.Rows
            && dto.SeatLayout.GetLength(1) == dto.Columns)
        {
            return true;
        }

        return false;
    } 
}