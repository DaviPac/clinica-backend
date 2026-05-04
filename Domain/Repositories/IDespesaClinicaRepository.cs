using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;

namespace Clinica.Domain.Repositories;

public interface IDespesaClinicaRepository
{
    Task CreateAsync(DespesaClinica despesa, CancellationToken ct);
    Task<IReadOnlyList<DespesaClinica>> ListAsync(FiltroDespesa filtro, CancellationToken ct = default);
    Task<Result> UpdatePagamentoAsync(int id, bool statusPagamento, CancellationToken ct = default);
}