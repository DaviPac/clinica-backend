using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Filters;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class DespesaClinicaRepository(AppDbContext db) : IDespesaClinicaRepository
{
    public async Task CreateAsync(DespesaClinica despesa, CancellationToken ct)
    {
        db.DespesasClinica.Add(despesa);
        await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<DespesaClinica>> ListAsync(FiltroDespesa filtro, CancellationToken ct = default)
    {
        var query = db.DespesasClinica.AsNoTracking().AsQueryable();

        if (filtro.De is DateOnly de)
            query = query.Where(d => d.DataVencimento >= de);

        if (filtro.Ate is DateOnly ate)
            query = query.Where(d => d.DataVencimento <= ate);

        if (filtro.Pago is bool pago)
            query = query.Where(d => d.StatusPagamento == pago);

        if (filtro.Categoria is CategoriaDespesa cat)
            query = query.Where(d => d.Categoria == cat);

        return await query
            .OrderBy(d => d.DataVencimento)
            .ToListAsync(ct);
    }
    public async Task<Result> UpdatePagamentoAsync(int id, bool statusPagamento, CancellationToken ct = default)
    {
        var rows = await db.DespesasClinica
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.StatusPagamento, statusPagamento), ct);
        if (rows == 0)
            return Errors.ExpenseNotFound;
        return Result.Success();
    }
}