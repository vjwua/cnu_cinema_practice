using AutoMapper;
using Core.DTOs.Seats;
using Core.Entities;

namespace Core.Mapping;

public class SeatMapping : Profile
{
    public SeatMapping()
    {
        CreateMap<Seat, SeatDTO>()
            .ForMember(sd => sd.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(sd => sd.HallId, opt => opt.MapFrom(s => s.HallId))
            .ForMember(sd => sd.SeatTypeId, opt => opt.MapFrom(s => s.SeatTypeId))
            .ForMember(sd => sd.RowNum, opt => opt.MapFrom(s => s.RowNum))
            .ForMember(sd => sd.SeatNum, opt => opt.MapFrom(s => s.SeatNum));
    }
}