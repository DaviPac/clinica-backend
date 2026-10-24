using Clinica.Application.Common;
using Clinica.Domain.Entities;

namespace Clinica.Domain.Repositories;

public interface IAnotacoesAgendamentoRepository
{
    Task CreateAsync(AnotacoesAgendamento anotacoesAgendamento, CancellationToken ct = default);
    Task<Result<AnotacoesAgendamento>> GetByAgendamentoIdAsync(int agendamentoId, CancellationToken ct = default);
    Task<Result> UpdateAsync(string anotacoesAgendamento, int agendamentoId, CancellationToken ct = default);
    Task<Result> DeleteAsync(int agendamentoId, CancellationToken ct = default);
}
