using AutoMapper;
using cnu_cinema_practice.ViewModels.Halls;
using Core.DTOs.Halls;
using Core.DTOs.Seats;

namespace cnu_cinema_practice.Mapping;

public class HallViewModelMapping : Profile
{
    public HallViewModelMapping()
    {
        CreateMap<HallListDTO, HallListViewModel>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Rows,
                opt => opt.MapFrom(src => src.Rows))
            .ForMember(dest => dest.Columns,
                opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name));

        CreateMap<HallDetailDTO, HallDetailViewModel>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Rows,
                opt => opt.MapFrom(src => src.Rows))
            .ForMember(dest => dest.Columns,
                opt => opt.MapFrom(src => src.Columns));

        CreateMap<CreateHallViewModel, CreateHallDTO>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Rows,
                opt => opt.MapFrom(src => src.Rows))
            .ForMember(dest => dest.Columns,
                opt => opt.MapFrom(src => src.Columns))
            .ForMember(dest => dest.SeatLayout,
                opt => opt.MapFrom(src => src.SeatLayout));

        CreateMap<UpdateHallViewModel, UpdateHallDTO>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SeatLayout,
                opt => opt.MapFrom(src => src.SeatLayout));

        CreateMap<SeatViewModel, SeatDTO>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HallId,
                opt => opt.MapFrom(src => src.HallId))
            .ForMember(dest => dest.RowNum,
                opt => opt.MapFrom(src => src.RowNum))
            .ForMember(dest => dest.SeatNum,
                opt => opt.MapFrom(src => src.SeatNum))
            .ForMember(dest => dest.SeatTypeId,
                opt => opt.MapFrom(src => src.SeatTypeId));
        
        CreateMap<SeatDTO, SeatViewModel>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HallId,
                opt => opt.MapFrom(src => src.HallId))
            .ForMember(dest => dest.RowNum,
                opt => opt.MapFrom(src => src.RowNum))
            .ForMember(dest => dest.SeatNum,
                opt => opt.MapFrom(src => src.SeatNum))
            .ForMember(dest => dest.SeatTypeId,
                opt => opt.MapFrom(src => src.SeatTypeId));

    }
}