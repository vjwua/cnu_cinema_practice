using Core.Enums;

namespace Core.Helpers;

public static class GenreMapper
{
    private static readonly Dictionary<string, MovieGenre> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Action"] = MovieGenre.Action,
        ["Adventure"] = MovieGenre.Adventure,
        ["Animation"] = MovieGenre.Animation,
        ["Biography"] = MovieGenre.Biography,
        ["Comedy"] = MovieGenre.Comedy,
        ["Crime"] = MovieGenre.Crime,
        ["Documentary"] = MovieGenre.Documentary,
        ["Drama"] = MovieGenre.Drama,
        ["Family"] = MovieGenre.Family,
        ["Fantasy"] = MovieGenre.Fantasy,
        ["Film Noir"] = MovieGenre.FilmNoir,
        ["FilmNoir"] = MovieGenre.FilmNoir,
        ["History"] = MovieGenre.History,
        ["Horror"] = MovieGenre.Horror,
        ["Music"] = MovieGenre.Music,
        ["Musical"] = MovieGenre.Musical,
        ["Mystery"] = MovieGenre.Mystery,
        ["Romance"] = MovieGenre.Romance,
        ["Science Fiction"] = MovieGenre.SciFi,
        ["Sci-Fi"] = MovieGenre.SciFi,
        ["SciFi"] = MovieGenre.SciFi,
        ["Short"] = MovieGenre.Short,
        ["Sport"] = MovieGenre.Sport,
        ["Superhero"] = MovieGenre.Superhero,
        ["Thriller"] = MovieGenre.Thriller,
        ["War"] = MovieGenre.War,
        ["Western"] = MovieGenre.Western,
        ["TV Movie"] = MovieGenre.None
    };

    public static MovieGenre ToGenreFlags(string? externalGenres)
    {
        if (string.IsNullOrWhiteSpace(externalGenres))
            return MovieGenre.None;

        var parts = externalGenres
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        MovieGenre result = MovieGenre.None;
        foreach (var part in parts)
        {
            if (Map.TryGetValue(part, out var mapped))
                result |= mapped;
        }

        return result;
    }

    public static string ToGenresText(IEnumerable<string>? genres)
    {
        if (genres == null) return string.Empty;
        return string.Join(", ", genres.Where(g => !string.IsNullOrWhiteSpace(g)).Select(g => g.Trim()));
    }
}
