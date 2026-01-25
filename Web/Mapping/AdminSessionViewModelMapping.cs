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
                opt => opt.MapFrom(src => src.MovieFormat.ToString()));

        CreateMap<SessionSeatDTO, SessionSeatViewModel>();

        CreateMap<SessionDetailDTO, SessionDetailsViewModel>()
            .ForMember(dest => dest.MovieFormat,
                opt => opt.MapFrom(src => src.MovieFormat.ToString()))
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
}