using AutoMapper;
using Core.DTOs.Payments;
using Core.Entities;

namespace Core.Mapping;

public class PaymentMapping : Profile
{
    public PaymentMapping()
    {
        CreateMap<Payment, PaymentDTO>()
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()));
    }
}