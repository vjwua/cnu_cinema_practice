using Core.Entities;

namespace Core.Interfaces.Repositories;

public interface IPersonRepository
{
    Task<Person?> GetByNameAsync(string name);
    Task<Person> CreateAsync(Person person);
    Task<List<Person>> GetByIdAsync(IEnumerable<int> ids);
}