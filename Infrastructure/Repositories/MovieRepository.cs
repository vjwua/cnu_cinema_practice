using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly CinemaDbContext _context;
    public MovieRepository(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _context.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();
    }

    public async Task<Movie?> GetByIdAsync(int id)
    {
        return await  _context.Movies
            .Include(m => m.Sessions)
            .Include(m => m.Actors)
            .Include(m => m.Directors)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Movie> CreateAsync(Movie movie)
    {
        await  _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    public async Task UpdateAsync(Movie movie)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Movie movie)
    {
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Movie>> GetByGenreAsync(MovieGenre genre)
    {
        return await _context.Movies
            .AsNoTracking()
            .Where(m => m.Genre == genre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Movie>();
        
        return await  _context.Movies
            .AsNoTracking()
            .Where(m => m.Name.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await  _context.Movies.AnyAsync(m => m.Id == id);
    }
}