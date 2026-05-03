using Api.Application.Common;

namespace Api.Domain;

public interface IDespesaClinicaRepository
{
    Task CreateAsync(DespesaClinica despesa, CancellationToken ct);
    Task<IReadOnlyList<DespesaClinica>> ListAsync(FiltroDespesa filtro, CancellationToken ct = default);
    Task<Result> UpdatePagamentoAsync(int id, bool statusPagamento, CancellationToken ct = default);
}