using Core.DTOs.Common;

namespace cnu_cinema_practice.ViewModels.Sessions;

public class AdminSessionIndexViewModel
{
    public required PagedResult<AdminSessionViewModel> Paged { get; init; }

    public string? Search { get; init; }
    public string? Format { get; init; }
    public string? DateFilter { get; init; }
}

