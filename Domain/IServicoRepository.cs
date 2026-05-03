using Clinica.Application.Common;
using Clinica.Application.DTOs;

namespace Clinica.Domain;

public interface IServicoRepository
{
    Task CreateAsync(Servico servico, CancellationToken ct = default);
    Task<Result<Servico>> FindByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Servico>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
    Task<IEnumerable<Servico>> ListAllAsync(bool mostrarInativos, CancellationToken ct = default);
    Task<IEnumerable<Servico>> ListByProfissionalAsync(int profissionalId, bool mostrarInativos, CancellationToken ct = default);
    Task<Result<Servico>> UpdateAsync(Servico servico, CancellationToken ct = default);
}