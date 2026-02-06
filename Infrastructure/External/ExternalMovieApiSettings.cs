namespace Infrastructure.External;

public sealed class ExternalMovieApiSettings
{
    public const string SectionName = "ExternalMovieApi";
    public const string HttpClientName = "ExternalMovieApi";
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string ApiKey { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    public int? TimeoutInSeconds { get; set; }

    public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p/w500";
    public string Language { get; set; } = "en-US";

    public int EffectiveTimeoutSeconds => TimeoutInSeconds ?? TimeoutSeconds;
}