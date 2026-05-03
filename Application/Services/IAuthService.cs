using Clinica.Application.Common;
using Clinica.Application.DTOs;
using Clinica.Domain;

namespace Clinica.Application.Services;

public interface IAuthService
{
    Task<Result<Usuario>> RegistrarAsync(RegistrarUsuarioRequest req, CancellationToken ct);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct);
    Task<IEnumerable<Usuario>> ListarUsuariosAsync(CancellationToken ct);
    Task<Result<Usuario>> ObterUsuarioPorIdAsync(int id, CancellationToken ct);
}