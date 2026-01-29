using Core.Enums;

namespace Core.DTOs.Sessions;

public class SessionAdminQueryDTO
{
    public string? Search { get; init; }
    public MovieFormat? MovieFormat { get; init; }

    public string? DateFilter { get; init; }

    public int Page { get; init; } = 1;
}

