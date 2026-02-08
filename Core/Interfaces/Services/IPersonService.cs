namespace Core.Interfaces.Services;

public interface IPersonService
{
    Task<int> GetOrCreatePersonIdByNameAsync(string name);
}