using AutoMapper;
using Core.DTOs.Movies;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly IPersonRepository _personRepository;
    
    public MovieService(IMovieRepository movieRepository, IMapper mapper, IPersonRepository personRepository)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _personRepository = personRepository;
    }

    public async Task<IEnumerable<MovieListDTO>> GetAllAsync()
    {
        var movies  = await _movieRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<MovieListDTO>>(movies);
    }

    public async Task<IEnumerable<MovieWithSessionsDTO>> GetAllWithUpcomingSessionsAsync()
    {
        var movies = await _movieRepository.GetAllWithUpcomingSessionsAsync();
        return _mapper.Map<IEnumerable<MovieWithSessionsDTO>>(movies);
    }

    public async Task<IEnumerable<MovieListDTO>> GetUpcomingMoviesAsync()
    {
        var movies = await _movieRepository.GetUpcomingMoviesAsync();
        return _mapper.Map<IEnumerable<MovieListDTO>>(movies);
    }

    public async Task<MovieDetailDTO?> GetByIdAsync(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null) return null;
        
        return _mapper.Map<MovieDetailDTO>(movie);
    }

    public async Task<IEnumerable<MovieListDTO>> GetByGenreAsync(MovieGenre genre)
    {
        var movies = await _movieRepository.GetByGenreAsync(genre);
        return _mapper.Map<IEnumerable<MovieListDTO>>(movies);
    }

    public async Task<MovieDetailDTO> CreateAsync(CreateMovieDTO dto)
    {
        var movie = _mapper.Map<Movie>(dto);
        
        if (dto.DirectorsIds.Any())
        {
            var directors = await _personRepository.GetByIdAsync(dto.DirectorsIds);
            movie.Directors = directors;
        }
        
        await _movieRepository.CreateAsync(movie);
        
        return _mapper.Map<MovieDetailDTO>(movie);
    }

    public async Task UpdateAsync(UpdateMovieDTO dto)
    {
        var movie = await _movieRepository.GetByIdAsync(dto.Id);
        
        if (movie == null)
            throw new KeyNotFoundException($"Movie with id {dto.Id} not found.");
        
        _mapper.Map(dto, movie);
        
        movie.Directors.Clear();
        if (dto.DirectorsIds.Any())
        {
            var directors = await _personRepository.GetByIdAsync(dto.DirectorsIds);
            foreach (var d in directors) movie.Directors.Add(d);
        }

        
        await _movieRepository.UpdateAsync(movie);
    }
    
    public async Task DeleteAsync(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        
        if (movie == null)
            throw new KeyNotFoundException($"Movie with id {id} not found.");
        
        await _movieRepository.DeleteAsync(movie);
    }

    public async Task<IEnumerable<MovieListDTO>> SearchByNameAsync(string searchTerm)
    {
        var movies = await _movieRepository.SearchByNameAsync(searchTerm);
        return _mapper.Map<IEnumerable<MovieListDTO>>(movies);
    }
}