using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Movies;
using Core.DTOs.Halls;
using Core.Enums;

namespace cnu_cinema_practice.Mapping;

public class AdminViewModelMapping : Profile
{
    public AdminViewModelMapping()
    {
        // Movie mappings
        CreateMap<MovieListDTO, AdminMovieViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating.HasValue ? src.ImdbRating.Value.ToString("F1") : "N/A"))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => "Unknown")) // Will be set from MovieDetailDTO
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => string.Empty)) // Not in ListDTO
            .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => new List<string> { src.Genre.ToString() }))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => true)); // Default value

        CreateMap<MovieDetailDTO, AdminMovieViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating.HasValue ? src.ImdbRating.Value.ToString("F1") : "N/A"))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => src.Directors.Any() ? src.Directors.First().Name : "Unknown"))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => new List<string> { src.Genre.ToString() }))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => true));

        CreateMap<MovieDetailDTO, MovieFormViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating.HasValue ? src.ImdbRating.Value.ToString("F1") : "N/A"))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => src.Directors.Any() ? src.Directors.First().Name : "Unknown"))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.GenresString,
                opt => opt.MapFrom(src => src.Genre.ToString()))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => true));

        CreateMap<MovieFormViewModel, CreateMovieDTO>()
            .ForMember(dest => dest.DurationMinutes,
                opt => opt.MapFrom(src => (ushort)src.DurationMinutes))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => ParseAgeLimit(src.ImdbRating)))
            .ForMember(dest => dest.Genre,
                opt => opt.MapFrom(src => ParseGenre(src.GenresString)))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ReleaseDate)))
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => ParseImdbRating(src.ImdbRating)))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.DirectorsIds,
                opt => opt.MapFrom(src => new List<int>()))
            .ForMember(dest => dest.ActorsIds,
                opt => opt.MapFrom(src => new List<int>()));

        CreateMap<MovieFormViewModel, UpdateMovieDTO>()
            .ForMember(dest => dest.DurationMinutes,
                opt => opt.MapFrom(src => (ushort)src.DurationMinutes))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => ParseAgeLimit(src.ImdbRating)))
            .ForMember(dest => dest.Genre,
                opt => opt.MapFrom(src => ParseGenre(src.GenresString)))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => DateOnly.FromDateTime(src.ReleaseDate)))
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => ParseImdbRating(src.ImdbRating)))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => (string?)null))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.DirectorsIds,
                opt => opt.MapFrom(src => new List<int>()))
            .ForMember(dest => dest.ActorsIds,
                opt => opt.MapFrom(src => new List<int>()));

        // Hall mappings
        CreateMap<HallListDTO, AdminHallViewModel>()
            .ForMember(dest => dest.SeatsPerRow,
                opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest => dest.AddedPrice,
                opt => opt.MapFrom(src => 0)) // Default value
            .ForMember(dest => dest.IsAvailable,
                opt => opt.MapFrom(src => true)); // Default value

        CreateMap<HallDetailDTO, HallFormViewModel>()
            .ForMember(dest => dest.SeatsPerRow,
                opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => true));

        CreateMap<HallFormViewModel, CreateHallDTO>()
            .ForMember(dest => dest.Rows,
                opt => opt.MapFrom(src => (byte)src.Rows))
            .ForMember(dest => dest.Columns,
                opt => opt.MapFrom(src => (byte)src.SeatsPerRow))
            .ForMember(dest => dest.SeatLayout,
                opt => opt.MapFrom(src => CreateDefaultSeatLayout((byte)src.Rows, (byte)src.SeatsPerRow)));

        CreateMap<HallFormViewModel, UpdateHallDTO>()
            .ForMember(dest => dest.SeatLayout,
                opt => opt.MapFrom(src => (byte[,]?)null)); // Don't update layout by default

        CreateMap<HallDetailDTO, HallLayoutViewModel>()
            .ForMember(dest => dest.HallId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HallName,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SeatsPerRow,
                opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest => dest.DisabledSeats,
                opt => opt.MapFrom(src => new List<string>())); // TODO: Map from actual seats
    }

    // Helper methods for custom conversions
    private static byte ParseAgeLimit(string imdbRating)
    {
        if (string.IsNullOrEmpty(imdbRating))
            return 0;

        var digits = new string(imdbRating.Where(char.IsDigit).ToArray());
        return byte.TryParse(digits, out var result) ? result : (byte)0;
    }

    private static MovieGenre ParseGenre(string genresString)
    {
        if (string.IsNullOrEmpty(genresString))
            return MovieGenre.Action;

        var firstGenre = genresString.Split(',').FirstOrDefault()?.Trim();
        return Enum.TryParse<MovieGenre>(firstGenre, out var genre)
            ? genre
            : MovieGenre.Action;
    }

    private static decimal? ParseImdbRating(string imdbRating)
    {
        if (string.IsNullOrEmpty(imdbRating) || imdbRating.Contains("+"))
            return null;

        return decimal.TryParse(imdbRating, out var result) ? result : null;
    }

    private static byte[,] CreateDefaultSeatLayout(byte rows, byte columns)
    {
        var layout = new byte[rows, columns];
        for (byte i = 0; i < rows; i++)
        {
            for (byte j = 0; j < columns; j++)
            {
                layout[i, j] = 1; // All seats active (1 = standard seat type)
            }
        }
        return layout;
    }
}