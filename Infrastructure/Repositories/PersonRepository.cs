using Core.Entities;
using Core.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly CinemaDbContext _context;
    
    public PersonRepository(CinemaDbContext context)
    {
        _context = context;
    }
    
    public Task<Person?> GetByNameAsync(string name)
    {
        var normalized = (name ?? string.Empty).Trim();
        return _context.People.FirstOrDefaultAsync(p => p.Name == normalized);
    }

    public async Task<Person> CreateAsync(Person person)
    {
        await _context.People.AddAsync(person);
        await _context.SaveChangesAsync();
        return person;
    }

    public Task<List<Person>> GetByIdAsync(IEnumerable<int> ids)
    {
        var list = ids
            .Where(x => x > 0)
            .Distinct()
            .ToList();
        
        return _context.People
            .Where(p => list.Contains(p.Id))
            .ToListAsync();
    }
}