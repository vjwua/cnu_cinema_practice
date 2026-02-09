using AutoMapper;
using Core.DTOs.Movies;
using Core.DTOs.Sessions;
using cnu_cinema_practice.ViewModels.Home;

namespace cnu_cinema_practice.Mapping;

public class HomeViewModelMapping : Profile
{
    public HomeViewModelMapping()
    {
        CreateMap<SessionListDTO, MovieSessionTimeViewModel>()
            .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.Id));

        CreateMap<MovieWithSessionsDTO, MovieCardViewModel>()
            .ForMember(dest => dest.ImdbRating,
                opt => opt.MapFrom(src => src.ImdbRating != null ? src.ImdbRating.Value.ToString("F1") : null))
            .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre.ToString()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.TrailerUrl, opt => opt.MapFrom(src => src.TrailerUrl))
            .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.Sessions.Any() ? src.Sessions.Min(s => s.BasePrice) : 0))
            .ForMember(dest => dest.Sessions, opt => opt.MapFrom(src => src.Sessions.Take(2)));

        CreateMap<MovieListDTO, UpcomingMovieViewModel>()
            .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre.ToString()))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty));
    }
}