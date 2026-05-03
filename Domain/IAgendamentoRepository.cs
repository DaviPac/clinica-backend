using Clinica.Application.Common;

namespace Clinica.Domain;

public interface IAgendamentoRepository
{
    Task CreateAsync(Agendamento agendamento, CancellationToken ct = default);
    Task CreateManyAsync(IEnumerable<Agendamento> lote, CancellationToken ct = default);
    Task<bool> CheckConflictAsync(int profissionalId, DateTimeOffset inicio, DateTimeOffset fim, CancellationToken ct = default);
    Task<bool> CheckAnyConflictAsync(int profissionalId, IEnumerable<(DateTimeOffset Inicio, DateTimeOffset Fim)> intervalos, CancellationToken ct = default);
    Task<Result<Agendamento>> FindByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Agendamento>> FindByIdForProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
    Task<IReadOnlyList<Agendamento>> ListAsync(FiltroAgendamento filtro, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdateStatusAsync(int id, StatusAgendamento status, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdateStatusForProfissionalAsync(int id, int profissionalId, StatusAgendamento status, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdatePagamentoAsync(int id, bool pago, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdatePagamentoForProfissionalAsync(int id, int profissionalId, bool pago, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdateValorCombinadoAsync(int id, decimal valorCombinado, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdateValorCombinadoForProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdateValorCombinadoRecorrenteAsync(int id, decimal valorCombinado, CancellationToken ct = default);
    Task<Result<Agendamento>> UpdateValorCombinadoRecorrenteForProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default);
    Task<Result> CancelRecorrenciaAsync(string recorrenciaGroupId, CancellationToken ct = default);
    Task<Result> CancelRecorrenciaForProfissionalAsync(string recorrenciaGroupId, int profissionalId, CancellationToken ct = default);
}