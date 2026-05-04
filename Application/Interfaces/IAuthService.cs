using Clinica.Application.Common;
using Clinica.Application.Features.Auth.DTOs;
using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces;

public interface IAuthService
{
    Task<Result<Usuario>> RegistrarAsync(RegistrarUsuarioRequest req, CancellationToken ct);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct);
    Task<IEnumerable<Usuario>> ListarUsuariosAsync(CancellationToken ct);
    Task<Result<Usuario>> ObterUsuarioPorIdAsync(int id, CancellationToken ct);
    Task<Result> MudarSenhaAsync(int id, string senhaAntiga, string novaSenha, CancellationToken ct = default);
}