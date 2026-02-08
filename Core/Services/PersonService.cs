using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }
    
    public async Task<int> GetOrCreatePersonIdByNameAsync(string name)
    {
        var normalized = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Director name cannot be empty.");

        var existing = await _personRepository.GetByNameAsync(normalized);
        if (existing != null)
            return existing.Id;

        var created = await _personRepository.CreateAsync(new Person
        {
            Name = normalized
        });
        
        return created.Id;
    }
}