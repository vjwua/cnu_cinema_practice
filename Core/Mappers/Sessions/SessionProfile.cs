using AutoMapper;
using Core.DTOs.Sessions;
using Core.Entities;

namespace Core.Mappers;

public class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<CreateSessionDTO, Session>();

        CreateMap<UpdateSessionDTO, Session>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Session, SessionDetailDTO>()
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Movie.Name))
            .ForMember(d => d.MovieDurationMinutes, o => o.MapFrom(s => s.Movie.DurationMinutes))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name))
            .ForMember(d => d.Seats, o => o.Ignore()); // will be completed with BuildSeats in service

        CreateMap<Session, SessionListDTO>()
            .ForMember(d => d.MovieName, o => o.MapFrom(s => s.Movie.Name))
            .ForMember(d => d.MoviePosterUrl, o => o.MapFrom(s => s.Movie.PosterUrl))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name));

        CreateMap<Session, SessionPreviewDTO>()
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Movie.Name))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name));
    }
}