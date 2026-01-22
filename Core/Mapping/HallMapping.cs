using AutoMapper;
using Core.Entities;
using Core.DTOs.Halls;

namespace Core.Mapping;

public class HallMapping : Profile
{
    public HallMapping()
    {
        CreateMap<Hall, HallListDTO>()
            .ForMember(hl => hl.Id, opt => opt.MapFrom(h => h.Id))
            .ForMember(hl => hl.Name, opt => opt.MapFrom(h => h.Name))
            .ForMember(hl => hl.Rows, opt => opt.MapFrom(h => h.Rows))
            .ForMember(hl => hl.Columns, opt => opt.MapFrom(h => h.Columns));

        CreateMap<Hall, HallDetailDTO>()
            .ForMember(hd => hd.Name, opt => opt.MapFrom(h => h.Name))
            .ForMember(hd => hd.Rows, opt => opt.MapFrom(h => h.Rows))
            .ForMember(hd => hd.Columns, opt => opt.MapFrom(h => h.Columns))
            .ForMember(hd => hd.Seats, opt => opt.MapFrom(h => h.Seats))
            .ForMember(hd => hd.Sessions, opt => opt.MapFrom(h => h.Sessions));

        CreateMap<CreateHallDTO, Hall>()
            .ForMember(h => h.Name, opt => opt.MapFrom(ch => ch.Name))
            .ForMember(h => h.Rows, opt => opt.MapFrom(ch => ch.Rows))
            .ForMember(h => h.Columns, opt => opt.MapFrom(ch => ch.Columns));
    }
}