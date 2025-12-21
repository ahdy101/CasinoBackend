using Casino_Api.Models;

namespace Casino_Api.Repositories.Factories;

public interface IRepositoryFactory
{
    T CreateRepository<T>() where T : class;
}
