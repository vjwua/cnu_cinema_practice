using AutoMapper;
using Core.Entities;
using Core.DTOs.Movies;

namespace Core.Mapping;

public class MovieMapping : Profile
{
    public MovieMapping()
    {
        CreateMap<Movie, MovieListDTO>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

        CreateMap<Movie, MovieWithSessionsDTO>();

        CreateMap<Movie, MovieDetailDTO>()
            .ForMember(x => x.Actors, opt => opt.MapFrom(x => x.Actors))
            .ForMember(x => x.Directors, opt => opt.MapFrom(x => x.Directors));

        CreateMap<Person, MoviePersonDTO>();

        CreateMap<CreateMovieDTO, Movie>()
            .ForMember(x => x.Actors, x => x.Ignore())
            .ForMember(x => x.Directors, x => x.Ignore());

        CreateMap<UpdateMovieDTO, Movie>()
            .ForMember(x => x.Actors, x => x.Ignore())
            .ForMember(x => x.Directors, x => x.Ignore())
            .ForMember(x => x.Sessions, x => x.Ignore());
    }
}