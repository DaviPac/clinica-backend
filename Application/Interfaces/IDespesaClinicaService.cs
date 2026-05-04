using Clinica.Application.Common;
using Clinica.Application.Features.Despesas.DTOs;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;

namespace Clinica.Application.Interfaces;

public interface IDespesaClinicaService
{
    Task<Result<DespesaClinica>> CriarDespesa(CriarDespesaRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<DespesaClinica>> ListarAsync(FiltroDespesa filtro, CancellationToken ct = default);
    Task<Result> AtualizarPagamentoAsync(int id, bool statusPagamento, CancellationToken ct = default);
}