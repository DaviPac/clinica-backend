namespace Clinica.Domain.Repositories;

public interface IUnitOfWork
{
    Task<bool> CommitAsync();
}