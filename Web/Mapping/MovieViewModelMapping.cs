using AutoMapper;
using Core.DTOs.External;
using Core.DTOs.Movies;
using Core.Enums;
using cnu_cinema_practice.ViewModels.Movies;

namespace cnu_cinema_practice.Mapping;

public class MovieViewModelMapping : Profile
{
    public MovieViewModelMapping()
    {
        CreateMap<MovieListDTO, MovieListViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => "Unknown"))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => new List<string> { src.Genre.ToString() }))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => src.TrailerUrl ?? string.Empty))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => src.AgeLimit));

        CreateMap<MovieDetailDTO, MovieDetailViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => src.Directors.Any() ? src.Directors.First().Name : "Unknown"))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => new List<string> { src.Genre.ToString() }))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => src.TrailerUrl ?? string.Empty))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => src.AgeLimit));

        CreateMap<MovieDetailDTO, CreateMovieViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => src.Directors.Any() ? src.Directors.First().Name : "Unknown"))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.GenresString,
                opt => opt.MapFrom(src => src.Genre.ToString()))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => src.TrailerUrl ?? string.Empty))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => src.AgeLimit));

        CreateMap<MovieDetailDTO, UpdateMovieViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.Director,
                opt => opt.MapFrom(src => src.Directors.Any() ? src.Directors.First().Name : "Unknown"))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.GenresString,
                opt => opt.MapFrom(src => src.Genre.ToString()))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => src.TrailerUrl ?? string.Empty))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => src.AgeLimit));

        CreateMap<CreateMovieViewModel, CreateMovieDTO>()
            .ForMember(dest => dest.DurationMinutes,
                opt => opt.MapFrom(src => (ushort)src.DurationMinutes))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => src.AgeLimit))
            .ForMember(dest => dest.Genre,
                opt => opt.MapFrom(src => ParseGenre(src.GenresString)))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate))
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => src.TrailerUrl))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.DirectorsIds,
                opt => opt.MapFrom(src => new List<int>()))
            .ForMember(dest => dest.ActorsIds,
                opt => opt.MapFrom(src => new List<int>()));

        CreateMap<UpdateMovieViewModel, UpdateMovieDTO>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DurationMinutes,
                opt => opt.MapFrom(src => (ushort)src.DurationMinutes))
            .ForMember(dest => dest.AgeLimit,
                opt => opt.MapFrom(src => src.AgeLimit))
            .ForMember(dest => dest.Genre,
                opt => opt.MapFrom(src => ParseGenre(src.GenresString)))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate))
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating))
            .ForMember(dest => dest.TrailerUrl,
                opt => opt.MapFrom(src => src.TrailerUrl ?? string.Empty))
            .ForMember(dest => dest.Country,
                opt => opt.MapFrom(src => src.Country))
            .ForMember(dest => dest.DirectorsIds,
                opt => opt.MapFrom(src => new List<int>()))
            .ForMember(dest => dest.ActorsIds,
                opt => opt.MapFrom(src => new List<int>()));

        CreateMap<ExternalMovieSearchResultDTO, ExternalMovieSearchViewModel>()
            .ForMember(dest => dest.Year,
                opt => opt.MapFrom(src => src.ReleaseDate.HasValue ? src.ReleaseDate.Value.Year : (int?)null));

        CreateMap<ExternalMovieDetailDTO, ImportMoviePreviewViewModel>()
            .ForMember(dest => dest.DurationMinutes,
                opt => opt.MapFrom(src => (int)src.DurationMinutes))
            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => src.ReleaseDate.HasValue
                    ? src.ReleaseDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null));
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
}
