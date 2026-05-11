using Clinica.Domain.Repositories;

namespace Clinica.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task<bool> CommitAsync()
    {
        return await db.SaveChangesAsync() > 0;
    }
}