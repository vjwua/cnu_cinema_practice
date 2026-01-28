using AutoMapper;
using Core.DTOs.Sessions;
using Core.Enums;
using cnu_cinema_practice.ViewModels;
using cnu_cinema_practice.ViewModels.Sessions;

namespace cnu_cinema_practice.Mapping;

public class AdminSessionViewModelMapping : Profile
{
    public AdminSessionViewModelMapping()
    {
        CreateMap<SessionListDTO, AdminSessionViewModel>()
            .ForMember(dest => dest.MovieFormat,
                opt => opt.MapFrom(src => FormatMovieFormat(src.MovieFormat)))
            .ForMember(dest => dest.MovieDuration,
                opt => opt.MapFrom(src => src.MovieDurationMinutes));

        CreateMap<SessionSeatDTO, SessionSeatViewModel>();

        CreateMap<SessionDetailDTO, SessionDetailsViewModel>()
            .ForMember(dest => dest.MovieId,
                opt => opt.MapFrom(src => src.MovieId)) // ← Додано MovieId
            .ForMember(dest => dest.HallId,
                opt => opt.MapFrom(src => src.HallId)) // ← Додано HallId
            .ForMember(dest => dest.MovieFormat,
                opt => opt.MapFrom(src => FormatMovieFormat(src.MovieFormat)))
            .AfterMap((src, dest) =>
            {
                foreach (var seat in dest.Seats)
                {
                    seat.TotalPrice = src.BasePrice + seat.AddedPrice;
                }
            });

        CreateMap<SessionDetailDTO, EditSessionViewModel>()
            .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => (int?)src.MovieId))
            .ForMember(dest => dest.HallId, opt => opt.MapFrom(src => (int?)src.HallId))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => (DateTime?)src.StartTime))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => (decimal?)src.BasePrice))
            .ForMember(dest => dest.MovieFormat, opt => opt.MapFrom(src => (MovieFormat?)src.MovieFormat));

        CreateMap<CreateSessionViewModel, CreateSessionDTO>();

        CreateMap<EditSessionViewModel, UpdateSessionDTO>();
    }

    private static string FormatMovieFormat(MovieFormat format)
    {
        return format switch
        {
            MovieFormat.TwoD => "2D",
            MovieFormat.ThreeD => "3D",
            MovieFormat.IMAX => "IMAX",
            MovieFormat.FourDX => "4DX",
            _ => format.ToString()
        };
    }
}