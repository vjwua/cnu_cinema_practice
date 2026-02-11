using AutoMapper;
using Core.DTOs.Orders;
using Core.Entities;

namespace Core.Mapping;

public class OrderMapping : Profile
{
    public OrderMapping()
    {
        CreateMap<Order, OrderDTO>()
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Session.Movie.Name))
            .ForMember(dest => dest.MoviePosterUrl, opt => opt.MapFrom(src => src.Session.Movie.PosterUrl))
            .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => src.Session.Hall.Name))
            .ForMember(dest => dest.SessionStart, opt => opt.MapFrom(src => src.Session.StartTime))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Tickets.Sum(t => t.Price)))
            .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment));

        CreateMap<Ticket, TicketDTO>()
            .ForMember(dest => dest.RowNum, opt => opt.MapFrom(src => src.SeatReservation.Seat.RowNum))
            .ForMember(dest => dest.SeatNum, opt => opt.MapFrom(src => src.SeatReservation.Seat.SeatNum))
            .ForMember(dest => dest.SeatType,
                opt => opt.MapFrom(src =>
                    src.SeatReservation.Seat.SeatType != null ? src.SeatReservation.Seat.SeatType.Name : "Standard"))
            .ForMember(dest => dest.QrCode, opt => opt.MapFrom(src => src.QrCodeBase64));
    }
}