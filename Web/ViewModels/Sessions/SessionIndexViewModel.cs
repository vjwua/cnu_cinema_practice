using Core.DTOs.Common;

namespace cnu_cinema_practice.ViewModels.Sessions;

public class SessionIndexViewModel
{
    public required PagedResult<SessionViewModel> Paged { get; init; }

    public string? Search { get; init; }
    public string? Format { get; init; }
    public string? DateFilter { get; init; }
}

