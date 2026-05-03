using Api.Application.Common;
using Api.Application.DTOs;
using Api.Domain;

namespace Api.Application.Services;

public class DespesaClinicaService(IDespesaClinicaRepository repo) : IDespesaClinicaService
{
    public async Task<Result<DespesaClinica>> CriarDespesa(DespesaClinicaDTO req, CancellationToken ct = default)
    {
        var despesa = new DespesaClinica
        {
            Descricao = req.Descricao,
            Valor = req.Valor,
            DataVencimento = req.DataVencimento,
            Categoria = req.Categoria
        };
        await repo.CreateAsync(despesa, ct);
        return despesa;
    }
    public async Task<IReadOnlyList<DespesaClinica>> ListarAsync(FiltroDespesa filtro, CancellationToken ct = default)
    {
        return await repo.ListAsync(filtro, ct);
    }
    public async Task<Result> AtualizarPagamentoAsync(int id, bool statusPagamento, CancellationToken ct = default)
    {
        return await repo.UpdatePagamentoAsync(id, statusPagamento, ct);
    }
}
