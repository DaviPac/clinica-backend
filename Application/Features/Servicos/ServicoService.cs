using Clinica.Application.Common;
using Clinica.Application.Features.Servicos.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Repositories;

namespace Clinica.Application.Features.Servicos;

public class ServicoService(IServicoRepository repo) : IServicoService
{
    public async Task<Result<Servico>> CriarAsync(int profissionalId, CriarServicoRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
        {
            return Errors.ValidationFailed("Nome não pode estar vazio");
        }
        if (req.ValorAtual < 0)
        {
            return Errors.ValidationFailed("Valor não pode ser negativo");
        }
        var servico = new Servico
        {
            Nome = req.Nome,
            ProfissionalId = profissionalId,
            Profissional = null!,
            ValorAtual = req.ValorAtual,
            Ativo = true,
            IsPacote = req.Pacote
        };
        await repo.CreateAsync(servico, ct);
        return servico;
    }

    public async Task<IEnumerable<Servico>> ListAllAsync(bool mostrarInativos, CancellationToken ct = default)
    {
        return await repo.ListAllAsync(mostrarInativos, ct);
    }

    public async Task<IEnumerable<Servico>> ListByProfissionalAsync(int profissionalId, bool mostrarInativos, CancellationToken ct = default)
    {
        return await repo.ListByProfissionalAsync(profissionalId, mostrarInativos, ct);
    }

    public async Task<Result<Servico>> AtualizarAsync(int id, AtualizarServicoRequest req, CancellationToken ct = default)
    {
        var servicoResult = await repo.FindByIdAsync(id, ct);

        if (!servicoResult.IsSuccess)
            return servicoResult.Error!;

        AplicarMudancas(servicoResult.Value!, req);

        return await repo.UpdateAsync(servicoResult.Value!, ct);
    }

    public async Task<Result<Servico>> AtualizarByProfissionalAsync(int id, int profissionalId, AtualizarServicoRequest req, CancellationToken ct = default)
    {
        var servicoResult = await repo.FindByIdAndProfissionalAsync(id, profissionalId, ct);

        if (!servicoResult.IsSuccess)
            return servicoResult.Error!;

        AplicarMudancas(servicoResult.Value!, req);

        return await repo.UpdateAsync(servicoResult.Value!, ct);
    }
    public async Task<Result<Servico>> DesativarAsync(int id, CancellationToken ct = default)
    {
        var servicoResult = await repo.FindByIdAsync(id, ct);

        if (!servicoResult.IsSuccess)
            return servicoResult.Error!;

        servicoResult.Value!.Ativo = false;

        return await repo.UpdateAsync(servicoResult.Value!, ct);
    }
    public async Task<Result<Servico>> DesativarByProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        var servicoResult = await repo.FindByIdAndProfissionalAsync(id, profissionalId, ct);

        if (!servicoResult.IsSuccess)
            return servicoResult.Error!;

        servicoResult.Value!.Ativo = false;

        return await repo.UpdateAsync(servicoResult.Value!, ct);
    }
    private static void AplicarMudancas(Servico servico, AtualizarServicoRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.Nome)) servico.Nome = req.Nome;
        if (req.ValorAtual is not null && req.ValorAtual >= 0) servico.ValorAtual = req.ValorAtual.Value;
        if (req.Ativo is not null) servico.Ativo = req.Ativo.Value;
        if (req.Pacote is not null) servico.IsPacote = req.Pacote.Value;
    }
}