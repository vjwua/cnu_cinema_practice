using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Orders;
using Core.DTOs.Payments;

namespace cnu_cinema_practice.Mapping;

public class PaymentViewModelMapping : Profile
{
    public PaymentViewModelMapping()
    {
        CreateMap<OrderDTO, PaymentViewModel>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalPrice))
            .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.MovieTitle))
            .ForMember(dest => dest.MoviePosterUrl, opt => opt.MapFrom(src => src.MoviePosterUrl))
            .ForMember(dest => dest.ShowDateTime, opt => opt.MapFrom(src => src.SessionStart))
            .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => src.HallName))
            .ForMember(dest => dest.AvailablePaymentMethods, opt => opt.Ignore())
            .ForMember(dest => dest.SelectedSeats, opt => opt.Ignore());

        CreateMap<PaymentViewModel, CreatePaymentDTO>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.SelectedPaymentMethod));
    }
}