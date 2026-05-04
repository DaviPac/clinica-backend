using Clinica.Application.Common;
using Clinica.Application.Features.Despesas.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;
using Clinica.Domain.Repositories;

namespace Clinica.Application.Services;

public class DespesaClinicaService(IDespesaClinicaRepository repo) : IDespesaClinicaService
{
    public async Task<Result<DespesaClinica>> CriarDespesa(CriarDespesaRequest req, CancellationToken ct = default)
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
