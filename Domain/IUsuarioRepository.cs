using Api.Application.Common;

namespace Api.Domain;

public interface IUsuarioRepository
{
    Task CreateAsync(Usuario usuario, string senhaHash, CancellationToken ct = default);
    Task<Result<(Usuario Usuario, string SenhaHash)>> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<Usuario>> FindByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Usuario>> ListAllAsync(CancellationToken ct = default);
    Task<Result> UpdateProfileAsync(int id, string nome, string profissao, CancellationToken ct = default);
    Task<Result> UpdatePasswordAsync(int id, string novaSenhaHash, CancellationToken ct = default);
    Task<Result> UpdateSystemRolesAsync(int id, Role role, decimal taxaComissao, CancellationToken ct = default);
    Task<Result> UpdateEmailAsync(int id, string novoEmail, CancellationToken ct = default);
}