using Api.Application.Common;
using Api.Application.DTOs;
using Api.Domain;

namespace Api.Application.Services;

public interface IServicoService
{
    Task<Result<Servico>> CriarAsync(int profissionalId, CriarServicoRequest req, CancellationToken ct = default);
    Task<IEnumerable<Servico>> ListAllAsync(bool mostrarInativos, CancellationToken ct = default);
    Task<IEnumerable<Servico>> ListByProfissionalAsync(int profissionalId, bool mostrarInativos, CancellationToken ct = default);
    Task<Result<Servico>> AtualizarAsync(int id, AtualizarServicoRequest req, CancellationToken ct = default);
    Task<Result<Servico>> AtualizarByProfissionalAsync(int id, int profissionalId, AtualizarServicoRequest req, CancellationToken ct = default);
    Task<Result<Servico>> DesativarAsync(int id, CancellationToken ct = default);
    Task<Result<Servico>> DesativarByProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
}