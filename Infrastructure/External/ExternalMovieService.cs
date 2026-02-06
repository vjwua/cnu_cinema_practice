using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DTOs.External;
using Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.External;

public sealed class ExternalMovieService : IExternalMovieService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalMovieApiSettings _settings;
    private readonly ILogger<ExternalMovieService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExternalMovieService(
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalMovieApiSettings> settings,
        ILogger<ExternalMovieService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new InvalidOperationException("ExternalMovieApi:ApiKey is not configured.");
    }

    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ExternalMovieApi");
            using var req = new HttpRequestMessage(HttpMethod.Get, $"configuration?api_key={Uri.EscapeDataString(_settings.ApiKey)}");
            using var resp = await client.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "External API availability check failed");
            return false;
        }
    }

    public async Task<IEnumerable<ExternalMovieSearchResultDTO>> SearchMoviesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<ExternalMovieSearchResultDTO>();

        try
        {
            var client = _httpClientFactory.CreateClient("ExternalMovieApi");
            var url = $"search/movie?api_key={Uri.EscapeDataString(_settings.ApiKey)}&query={Uri.EscapeDataString(query)}&language={Uri.EscapeDataString(_settings.Language)}";

            using var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                await LogNonSuccess("SearchMovies", resp);
                return Enumerable.Empty<ExternalMovieSearchResultDTO>();
            }

            await using var stream = await resp.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<TmdbSearchResponse>(stream, JsonOptions);

            var results = (data?.Results ?? new List<TmdbSearchMovie>())
                .Take(10)
                .Select(r => new ExternalMovieSearchResultDTO
                {
                    ExternalId = r.Id,
                    Name = r.Title ?? string.Empty,
                    PosterUrl = BuildPosterUrl(r.PosterPath),
                    ReleaseDate = TryParseDateOnly(r.ReleaseDate)
                })
                .Where(r => r.ExternalId > 0 && !string.IsNullOrWhiteSpace(r.Name));

            return results.ToList();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "External API timeout during movie search");
            return Enumerable.Empty<ExternalMovieSearchResultDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API error during movie search");
            return Enumerable.Empty<ExternalMovieSearchResultDTO>();
        }
    }

    public async Task<ExternalMovieDetailDTO?> GetMovieDetailsAsync(int externalId)
    {
        if (externalId <= 0)
            return null;

        try
        {
            var client = _httpClientFactory.CreateClient("ExternalMovieApi");

            var detailsUrl = $"movie/{externalId}?api_key={Uri.EscapeDataString(_settings.ApiKey)}&language={Uri.EscapeDataString(_settings.Language)}";
            using var detailsResp = await client.GetAsync(detailsUrl);
            if (detailsResp.StatusCode == HttpStatusCode.NotFound)
                return null;
            if (!detailsResp.IsSuccessStatusCode)
            {
                await LogNonSuccess("GetMovieDetails", detailsResp);
                return null;
            }
            await using var detailsStream = await detailsResp.Content.ReadAsStreamAsync();
            var details = await JsonSerializer.DeserializeAsync<TmdbMovieDetails>(detailsStream, JsonOptions);
            if (details == null)
                return null;

            var creditsUrl = $"movie/{externalId}/credits?api_key={Uri.EscapeDataString(_settings.ApiKey)}&language={Uri.EscapeDataString(_settings.Language)}";
            var credits = await SafeGetAsync<TmdbCredits>(client, creditsUrl);

            var videosUrl = $"movie/{externalId}/videos?api_key={Uri.EscapeDataString(_settings.ApiKey)}&language={Uri.EscapeDataString(_settings.Language)}";
            var videos = await SafeGetAsync<TmdbVideos>(client, videosUrl);

            var releaseDatesUrl = $"movie/{externalId}/release_dates?api_key={Uri.EscapeDataString(_settings.ApiKey)}";
            var releaseDates = await SafeGetAsync<TmdbReleaseDates>(client, releaseDatesUrl);

            var genreNames = (details.Genres ?? new List<TmdbGenre>())
                .Select(g => g.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!.Trim())
                .ToList();

            var directors = (credits?.Crew ?? new List<TmdbCrew>())
                .Where(c => string.Equals(c.Job, "Director", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList() ?? new List<string>();

            var actors = (credits?.Cast ?? new List<TmdbCast>())
                .Select(c => c.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(20)
                .ToList() ?? new List<string>();

            var trailerUrl = BuildTrailerUrl(videos);
            var country = details.ProductionCountries?.FirstOrDefault()?.Name;
            var ageLimit = ParseAgeLimit(releaseDates);

            var dto = new ExternalMovieDetailDTO
            {
                ExternalId = details.Id,
                Name = details.Title ?? string.Empty,
                Description = details.Overview,
                DurationMinutes = (ushort)Math.Clamp(details.Runtime ?? 0, 0, ushort.MaxValue),
                AgeLimit = ageLimit,
                GenresText = string.Join(", ", genreNames),
                ReleaseDate = TryParseDateOnly(details.ReleaseDate),
                ImdbRating = details.VoteAverage.HasValue ? Math.Round((decimal)details.VoteAverage.Value, 1) : null,
                PosterUrl = BuildPosterUrl(details.PosterPath),
                TrailerUrl = trailerUrl,
                Country = country,
                Directors = directors,
                Actors = actors
            };

            return dto;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "External API timeout during movie details fetch (id={ExternalId})", externalId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API error during movie details fetch (id={ExternalId})", externalId);
            return null;
        }
    }

    private async Task<T?> SafeGetAsync<T>(HttpClient client, string relativeUrl) where T : class
    {
        try
        {
            using var resp = await client.GetAsync(relativeUrl);
            if (!resp.IsSuccessStatusCode)
            {
                await LogNonSuccess(typeof(T).Name, resp);
                return null;
            }
            await using var stream = await resp.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogDebug(ex, "External API call failed for {Url}", relativeUrl);
            return null;
        }
    }

    private async Task LogNonSuccess(string operation, HttpResponseMessage response)
    {
        var body = string.Empty;
        try { body = await response.Content.ReadAsStringAsync(); } catch { /* ignore */ }

        _logger.LogWarning(
            "External API non-success in {Operation}: {StatusCode} {Reason}. Body: {Body}",
            operation,
            (int)response.StatusCode,
            response.ReasonPhrase,
            body);
    }

    private string? BuildPosterUrl(string? posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath))
            return null;
        var baseUrl = _settings.ImageBaseUrl.TrimEnd('/');
        var path = posterPath.TrimStart('/');
        return $"{baseUrl}/{path}";
    }

    private static DateOnly? TryParseDateOnly(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;
        return DateOnly.TryParse(date, out var d) ? d : null;
    }

    private static string? BuildTrailerUrl(TmdbVideos? videos)
    {
        var v = (videos?.Results ?? new List<TmdbVideo>())
            .Where(x => string.Equals(x.Site, "YouTube", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => string.Equals(x.Type, "Trailer", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => x.Official == true)
            .FirstOrDefault();

        if (v == null || string.IsNullOrWhiteSpace(v.Key))
            return null;

        return $"https://www.youtube.com/watch?v={v.Key}";
    }

    private static byte ParseAgeLimit(TmdbReleaseDates? releaseDates)
    {
        var all = releaseDates?.Results ?? new List<TmdbReleaseDatesResult>();
        var us = all.FirstOrDefault(r => string.Equals(r.Iso3166_1, "US", StringComparison.OrdinalIgnoreCase));
        var cert = us?.ReleaseDates?.Select(x => x.Certification).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));

        cert ??= all.SelectMany(r => r.ReleaseDates ?? new List<TmdbReleaseDateItem>())
            .Select(x => x.Certification)
            .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c));

        if (string.IsNullOrWhiteSpace(cert))
            return 0;

        cert = cert.Trim().ToUpperInvariant();

        return cert switch
        {
            "G" => 0,
            "PG" => 0,
            "PG-13" => 12,
            "R" => 16,
            "NC-17" => 18,
            _ => TryParseDigits(cert)
        };
    }

    private static byte TryParseDigits(string cert)
    {
        var digits = new string(cert.Where(char.IsDigit).ToArray());
        if (byte.TryParse(digits, out var v))
            return v;
        return 0;
    }

    private sealed class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbSearchMovie>? Results { get; set; }
    }

    private sealed class TmdbSearchMovie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }
    }

    private sealed class TmdbMovieDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double? VoteAverage { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenre>? Genres { get; set; }

        [JsonPropertyName("production_countries")]
        public List<TmdbCountry>? ProductionCountries { get; set; }
    }

    private sealed class TmdbGenre
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class TmdbCountry
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class TmdbCredits
    {
        [JsonPropertyName("cast")]
        public List<TmdbCast>? Cast { get; set; }
        [JsonPropertyName("crew")]
        public List<TmdbCrew>? Crew { get; set; }
    }

    private sealed class TmdbCast
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class TmdbCrew
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("job")]
        public string? Job { get; set; }
    }

    private sealed class TmdbVideos
    {
        [JsonPropertyName("results")]
        public List<TmdbVideo>? Results { get; set; }
    }

    private sealed class TmdbVideo
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        [JsonPropertyName("site")]
        public string? Site { get; set; }
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("official")]
        public bool? Official { get; set; }
    }

    private sealed class TmdbReleaseDates
    {
        [JsonPropertyName("results")]
        public List<TmdbReleaseDatesResult>? Results { get; set; }
    }

    private sealed class TmdbReleaseDatesResult
    {
        [JsonPropertyName("iso_3166_1")]
        public string? Iso3166_1 { get; set; }
        [JsonPropertyName("release_dates")]
        public List<TmdbReleaseDateItem>? ReleaseDates { get; set; }
    }

    private sealed class TmdbReleaseDateItem
    {
        [JsonPropertyName("certification")]
        public string? Certification { get; set; }
    }
}
