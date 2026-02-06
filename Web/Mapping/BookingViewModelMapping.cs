using System.Security;
using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Sessions;
using Core.DTOs.Seats;

namespace cnu_cinema_practice.Mapping;

public class BookingViewModelMapping : Profile
{
    public BookingViewModelMapping()
    {
        // Session to ShowtimeOption mapping
        CreateMap<SessionListDTO, ShowtimeOption>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DateTime,
                opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.Hall,
                opt => opt.MapFrom(src => src.HallName));

        // Session to BookingViewModel (partial - needs movie info too)
        CreateMap<SessionDetailDTO, BookingViewModel>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.MovieId))
            .ForMember(dest => dest.ShowtimeId,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ShowDateTime,
                opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.HallData,
                opt => opt.Ignore())
            .ForMember(dest => dest.HallId,
                opt => opt.MapFrom(src => src.HallId))
            .ForMember(dest => dest.BasePrice,
                opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(dest => dest.Name,
                opt => opt.Ignore()) // Set from movie
            .ForMember(dest => dest.PosterUrl,
                opt => opt.Ignore()) // Set from movie
            .ForMember(dest => dest.AvailableShowtimes,
                opt => opt.Ignore()) // Set separately
            .ForMember(dest => dest.SeatLayout,
                opt => opt.Ignore())
            .ForMember(dest => dest.LayoutArray,
                opt => opt.Ignore())
            .ForMember(dest => dest.alertMessage,
                opt => opt.Ignore())
            .ForMember(dest => dest.addedPrice,
                opt => opt.Ignore()); // Set separately

        // Session to CheckoutViewModel (partial)
        CreateMap<SessionDetailDTO, CheckoutViewModel>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.MovieId))
            .ForMember(dest => dest.ShowDateTime,
                opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.Hall,
                opt => opt.MapFrom(src => src.HallName))
            .ForMember(dest => dest.BasePrice,
                opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(dest => dest.Name,
                opt => opt.Ignore()) // Set from movie
            .ForMember(dest => dest.PosterUrl,
                opt => opt.Ignore()) // Set from movie
            .ForMember(dest => dest.SelectedSeats,
                opt => opt.Ignore()) // Set from user selection
            .ForMember(dest => dest.ServiceFee,
                opt => opt.MapFrom(src => 2.50m)) // Fixed service fee
            .ForMember(dest => dest.FullName,
                opt => opt.Ignore())
            .ForMember(dest => dest.Email,
                opt => opt.Ignore())
            .ForMember(dest => dest.Phone,
                opt => opt.Ignore());

        // Seats to SeatLayout mapping
        CreateMap<IEnumerable<SeatDTO>, SeatLayout>()
            .ForMember(dest => dest.Rows,
                opt => opt.MapFrom(src => src.Any() ? src.Max(s => s.RowNum) : (byte)0))
            .ForMember(dest => dest.SeatsPerRow,
                opt => opt.MapFrom(src => src.Any() ? src.Max(s => s.SeatNum) : (byte)0))
            .ForMember(dest => dest.AvailableSeats,
                opt => opt.Ignore()); // Set separately based on reservations
    }
}