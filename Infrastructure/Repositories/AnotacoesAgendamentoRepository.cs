using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class AnotacoesAgendamentoRepository(AppDbContext db) : IAnotacoesAgendamentoRepository
{
    public async Task CreateAsync(AnotacoesAgendamento anotacoesAgendamento, CancellationToken ct = default)
    {
        db.AnotacoesAgendamentos.Add(anotacoesAgendamento);
        await db.SaveChangesAsync(ct);
    }

    public async Task<Result<AnotacoesAgendamento>> GetByAgendamentoIdAsync(int agendamentoId, CancellationToken ct = default) {
        var anotacoesAgendamento = await db.AnotacoesAgendamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.AgendamentoId == agendamentoId, ct);
        if (anotacoesAgendamento is null)
        {
            return Errors.NotesNotFound;
        }
        return anotacoesAgendamento;
    }

    public async Task<Result> UpdateAsync(string anotacoesAgendamento, int agendamentoId, CancellationToken ct = default)
    {
        var rows = await db.AnotacoesAgendamentos
            .Where(p => p.AgendamentoId == agendamentoId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Conteudo, anotacoesAgendamento),
            ct);
        if (rows == 0)
        {
            return Errors.NotesNotFound;
        }
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int agendamentoId, CancellationToken ct = default)
    {
        var rows = await db.AnotacoesAgendamentos
            .Where(p => p.AgendamentoId == agendamentoId)
            .ExecuteDeleteAsync(ct);
        if (rows == 0)
        {
            return Errors.NotesNotFound;
        }
        return Result.Success();
    }
}
