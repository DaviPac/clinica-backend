using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinica.Application.Common;
using Clinica.Application.Features.Auth.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Clinica.Application.Services;

public class AuthService(IUsuarioRepository repo, IConfiguration config) : IAuthService
{
    public async Task<Result<Usuario>> RegistrarAsync(RegistrarUsuarioRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nome) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Senha))
            return Errors.ValidationFailed("Nome, email e senha são obrigatórios.");

        var role = Role.PROFISSIONAL;
        if (req.Role == Role.ADMIN)
            role = Role.ADMIN;

        if (!Enum.IsDefined(role))
            return Errors.ValidationFailed("Role inválido.");

        var emailEmUsoResult = await repo.FindByEmailAsync(req.Email, ct);
        if (emailEmUsoResult.IsSuccess)
            return Errors.EmailAlreadyInUse;

        decimal taxaDefault = 40;

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(req.Senha);

        var usuario = new Usuario
        {
            Nome               = req.Nome,
            Email              = req.Email,
            SenhaHash          = senhaHash,
            Role               = role,
            Profissao          = req.Profissao,
            TaxaComissaoPadrao = req.TaxaComissaoPadrao ?? taxaDefault,
        };

        await repo.CreateAsync(usuario, senhaHash, ct);
        
        return usuario;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await repo.FindByEmailAsync(req.Email, ct);

        if (!result.IsSuccess || !BCrypt.Net.BCrypt.Verify(req.Senha, result.Value.SenhaHash))
            return Errors.InvalidCredentials; 

        var token = GerarToken(result.Value.Usuario);

        var u = result.Value.Usuario;
        return new LoginResponse(token, new UsuarioResponse(
            u.Id,
            u.Nome,
            u.Email,
            u.Role.ToString(),
            u.Profissao,
            u.TaxaComissaoPadrao,
            u.CriadoEm
        ));
    }

    public async Task<IEnumerable<Usuario>> ListarUsuariosAsync(CancellationToken ct)
    {
        return await repo.ListAllAsync(ct);
    }

    public async Task<Result<Usuario>> ObterUsuarioPorIdAsync(int id, CancellationToken ct)
    {
        return await repo.FindByIdAsync(id, ct);
    }

    public async Task<Result> MudarSenhaAsync(int id, string senhaAntiga, string novaSenha, CancellationToken ct = default)
    {
        if (novaSenha.Length < 5)
            return Errors.ValidationFailed("Senha deve ter ao menos 5 dígitos.");
        var usuarioResult = await repo.FindByIdAsync(id, ct);
        if (!usuarioResult.IsSuccess)
            return usuarioResult.Error!;
        if (!BCrypt.Net.BCrypt.Verify(senhaAntiga, usuarioResult.Value!.SenhaHash))
            return Errors.ValidationFailed("Senha incorreta.");
        var novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        return await repo.UpdatePasswordAsync(id, novaSenhaHash, ct);
    }

    private string GerarToken(Usuario u)
    {
        var secret = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT_SECRET não configurado.");
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("user_id", u.Id.ToString()),
            new Claim("role",    u.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}