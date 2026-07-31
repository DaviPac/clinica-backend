using Clinica.Application.Common;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Repositories;
using Clinica.Application.Features.Usuarios.DTOs;
using Clinica.Domain.Enums;

namespace Clinica.Application.Features.Usuarios;

public class UsuarioService(IUsuarioRepository repo, IUnitOfWork unitOfWork) : IUsuarioService
{
    public async Task<IEnumerable<Usuario>> ListarUsuariosAsync(CancellationToken ct)
    {
        return await repo.ListAllAsync(ct);
    }

    public async Task<Result<Usuario>> ObterUsuarioPorIdAsync(int id, CancellationToken ct)
    {
        return await repo.FindByIdAsync(id, ct);
    }

    public async Task<Result<Usuario>> AtualizarUsuarioPorIdAsync(int id, AtualizarUsuarioRequest request, CancellationToken ct)
    {
        var usuarioResult  = await repo.FindByIdTrackingAsync(id, ct);
        if (!usuarioResult.IsSuccess)
        {
            return usuarioResult.Error!;
        }
        var usuario = usuarioResult.Value!;
        if (request.Nome is not null)
        {
            usuario.Nome = request.Nome;
        }
        if (request.Email is not null)
        {
            usuario.Email = request.Email;
        }
        if (request.Profissao is not null)
        {
            usuario.Profissao = request.Profissao;
        }
        if (request.Role is not null)
        {
            usuario.Role = request.Role == "ADMIN" ? Role.ADMIN : Role.PROFISSIONAL;
        }
        if (request.TaxaComissaoPadrao.HasValue)
        {
            usuario.TaxaComissaoPadrao = request.TaxaComissaoPadrao.Value;
        }
        if (request.ProfissionalRecebe.HasValue)
        {
            usuario.ProfissionalRecebe = request.ProfissionalRecebe.Value;
        }
        await unitOfWork.CommitAsync();
        return usuario;
    }
}