using Clinica.Application.Common;
using Clinica.Application.Features.Usuarios.DTOs;
using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces;

public interface IUsuarioService
{
    Task<IEnumerable<Usuario>> ListarUsuariosAsync(CancellationToken ct);
    Task<Result<Usuario>> ObterUsuarioPorIdAsync(int id, CancellationToken ct);
    Task<Result<Usuario>> AtualizarUsuarioPorIdAsync(int id, AtualizarUsuarioRequest request, CancellationToken ct);
}