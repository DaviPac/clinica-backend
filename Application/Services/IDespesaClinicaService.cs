using Clinica.Application.Common;
using Clinica.Application.DTOs;
using Clinica.Domain;

namespace Clinica.Application.Services;

public interface IDespesaClinicaService
{
    Task<Result<DespesaClinica>> CriarDespesa(DespesaClinicaDTO req, CancellationToken ct = default);
    Task<IReadOnlyList<DespesaClinica>> ListarAsync(FiltroDespesa filtro, CancellationToken ct = default);
    Task<Result> AtualizarPagamentoAsync(int id, bool statusPagamento, CancellationToken ct = default);
}