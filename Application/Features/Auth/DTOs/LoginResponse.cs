namespace Clinica.Application.Features.Auth.DTOs;

public record LoginResponse(string Token, UsuarioResponse Usuario);