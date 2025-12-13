using Casino_Api.Models;

namespace Casino_Api.Factories.Interfaces;

public interface IRepositoryFactory
{
    T CreateRepository<T>() where T : class;
}
