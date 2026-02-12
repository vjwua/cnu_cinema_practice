using AutoMapper;
using Core.DTOs.Orders;
using Core.Enums;
using cnu_cinema_practice.ViewModels.Account;

namespace cnu_cinema_practice.Mapping;

public class OrderProfileViewModelMapping : Profile
{
    public OrderProfileViewModelMapping()
    {
        CreateMap<TicketDTO, TicketViewModel>()
            .ForMember(dest => dest.SeatTypeName, opt => opt.MapFrom(src => src.SeatType))
            .ForMember(dest => dest.QrCode, opt => opt.MapFrom(src => src.QrCode))
            .ForMember(dest => dest.SeatNum, opt => opt.MapFrom(src => src.SeatNum + 1))
            .ForMember(dest => dest.RowNum, opt => opt.MapFrom(src => ((char)('A' + src.RowNum)).ToString()));

        CreateMap<OrderDTO, OrderViewModel>()
            .ForMember(dest => dest.MovieName, opt => opt.MapFrom(src => src.MovieTitle))
            .ForMember(dest => dest.MoviePosterUrl, opt => opt.MapFrom(src => src.MoviePosterUrl))
            .ForMember(dest => dest.SessionStartTime, opt => opt.MapFrom(src => src.SessionStart))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalPrice))
            .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => src.HallName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ParseStatus(src.Status)))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.Payment != null))
            .ForMember(dest => dest.PaymentMethod,
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.PaymentMethod : null))
            .ForMember(dest => dest.PaidAt,
                opt => opt.MapFrom(src => src.Payment != null ? (DateTime?)src.Payment.PaidAt : null));
    }

    private static OrderStatus ParseStatus(string status)
    {
        return Enum.TryParse<OrderStatus>(status, out var parsed) ? parsed : OrderStatus.Created;
    }
}