using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.ReadModels;

namespace Clinica.Domain.Repositories;

public interface IProntuarioRepository
{
    Task CreateAsync(Prontuario prontuario, CancellationToken ct = default);
    Task<bool> ExistsByAgendamentoIdAsync(int agendamentoId, CancellationToken ct = default);
    Task<Result<ProntuarioVersao>> GetVersaoAtualAsync(int agendamentoId, CancellationToken ct = default);
    Task<Result<ProntuarioVersao>> GetVersaoAsync(int agendamentoId, int versaoId, CancellationToken ct = default);
    Task<IReadOnlyList<ProntuarioVersaoResumo>> ListVersoesAsync(int agendamentoId, CancellationToken ct = default);
    Task<Result> AddVersaoAsync(int agendamentoId, string conteudo, int autorId, CancellationToken ct = default);}
