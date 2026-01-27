using AutoMapper;
using Core.DTOs.Sessions;
using Core.Entities;

namespace Core.Mapping;

public class SessionMapping : Profile
{
    public SessionMapping()
    {
        CreateMap<CreateSessionDTO, Session>();

        CreateMap<UpdateSessionDTO, Session>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Session, SessionDetailDTO>()
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Movie.Name))
            .ForMember(d => d.MovieDurationMinutes, o => o.MapFrom(s => s.Movie.DurationMinutes))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name))
            .ForMember(d => d.Seats, o => o.Ignore()); // Місця будуть заповнені в сервісі

        CreateMap<Session, SessionListDTO>()
            .ForMember(d => d.MovieName, o => o.MapFrom(s => s.Movie.Name))
            .ForMember(d => d.MoviePosterUrl, o => o.MapFrom(s => s.Movie.PosterUrl))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name))
            .ForMember(d => d.MovieDurationMinutes, o => o.MapFrom(s => s.Movie.DurationMinutes))
            .ForMember(d => d.TotalSeats, o => o.MapFrom(s => s.Hall.Seats.Count))
            .ForMember(d => d.OccupiedSeats, o => o.MapFrom(s => s.SeatReservations.Count));

        CreateMap<Session, SessionPreviewDTO>()
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Movie.Name))
            .ForMember(d => d.HallName, o => o.MapFrom(s => s.Hall.Name));
    }
}