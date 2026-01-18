using Core.Entities;

namespace Core.Enums;

public enum MovieGenre : ulong
{
    None = 0,
    Action = 1,
    Adventure = 2,
    Animation = 4,
    Biography = 8,
    Comedy = 16,
    Crime = 32,
    Documentary = 64,
    Drama = 128,
    Family = 256,
    Fantasy = 512,
    FilmNoir = 1024,
    History = 2048,
    Horror = 4096,
    Music = 8192,
    Musical = 16_384,
    Mystery = 32_768,
    Romance = 65_536,
    SciFi = 131_072,
    Short = 262_144,
    Sport = 524_288,
    Superhero = 1_048_576,
    Thriller = 2_097_152,
    War = 4_194_304,
    Western = 8_388_608
}

public class GenreConverter
{
    public static Dictionary<string, MovieGenre> EnglishToGenre = new Dictionary<string, MovieGenre>()
    {
        { "Action", MovieGenre.Action },
        { "Adventure", MovieGenre.Adventure },
        { "Animation", MovieGenre.Animation },
        { "Biography", MovieGenre.Biography },
        { "Comedy", MovieGenre.Comedy },
        { "Crime", MovieGenre.Crime },
        { "Documentary", MovieGenre.Documentary },
        { "Drama", MovieGenre.Drama },
        { "Family", MovieGenre.Family },
        { "Fantasy", MovieGenre.Fantasy },
        { "FilmNoir", MovieGenre.FilmNoir },
        { "History", MovieGenre.History },
        { "Horror", MovieGenre.Horror },
        { "Music", MovieGenre.Music },
        { "Musical", MovieGenre.Musical },
        { "Mystery", MovieGenre.Mystery },
        { "Romance", MovieGenre.Romance },
        { "SciFi", MovieGenre.SciFi },
        { "Short", MovieGenre.Short },
        { "Sport", MovieGenre.Sport },
        { "Superhero", MovieGenre.Superhero },
        { "Thriller", MovieGenre.Thriller },
        { "War", MovieGenre.War },
        { "Western", MovieGenre.Western }
    };
    
    // TODO? genre number to UA name
    
    static MovieGenre ToGenre(string data)
    {
        if (EnglishToGenre.ContainsKey(data)) return EnglishToGenre[data];

        return MovieGenre.None;
    }

    static List<MovieGenre> ToGenre(List<string> data)
    {
        List<MovieGenre> genreList = new List<MovieGenre>();
        foreach (var se in data) genreList.Append(ToGenre(se));

        return genreList;
    }
    
}


// source: https://github.com/weirdyang/movie-therapy-angular/blob/feature/filter-menu/src/assets/shows/genres.json
