using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class UsuarioRepository(AppDbContext db) : IUsuarioRepository
{
    public async Task CreateAsync(Usuario usuario, string senhaHash, CancellationToken ct = default)
    {
        usuario.SenhaHash = senhaHash;
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);
    }

    public async Task<Result<(Usuario Usuario, string SenhaHash)>> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (usuario is null) return Errors.AccountNotFound;

        return (usuario, usuario.SenhaHash);
    }

    public async Task<Result<Usuario>> FindByIdAsync(int id, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (usuario is null) return Errors.AccountNotFound;

        return usuario;
    }

    public async Task<IEnumerable<Usuario>> ListAllAsync(CancellationToken ct = default)
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ToListAsync(ct);

        return usuarios; 
    }

    public async Task<Result> UpdateProfileAsync(int id, string nome, string profissao, CancellationToken ct = default)
    {
        var affected = await db.Usuarios
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Nome, nome)
                .SetProperty(u => u.Profissao, profissao),
            ct);

        return CheckAffectedRows(affected);
    }

    public async Task<Result> UpdatePasswordAsync(int id, string novaSenhaHash, CancellationToken ct = default)
    {
        var affected = await db.Usuarios
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.SenhaHash, novaSenhaHash),
            ct);

        return CheckAffectedRows(affected);
    }

    public async Task<Result> UpdateSystemRolesAsync(int id, Role role, decimal taxaComissao, CancellationToken ct = default)
    {
        var affected = await db.Usuarios
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Role, role)
                .SetProperty(u => u.TaxaComissaoPadrao, taxaComissao),
            ct);

        return CheckAffectedRows(affected);
    }

    public async Task<Result> UpdateEmailAsync(int id, string novoEmail, CancellationToken ct = default)
    {
        var affected = await db.Usuarios
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Email, novoEmail),
            ct);

        return CheckAffectedRows(affected);
    }

    private static Result CheckAffectedRows(int affected)
    {
        if (affected == 0)
            return Errors.AccountNotFound;

        return Result.Success();
    }
}