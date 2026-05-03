using Api.Domain;

namespace Api.Application.DTOs;

public record RegistrarUsuarioRequest(
    string  Nome,
    string  Email,
    string  Senha,
    Role?    Role,
    string? Profissao,
    decimal? TaxaComissaoPadrao
);

public record LoginRequest(string Email, string Senha);

public record LoginResponse(string Token, Usuario Usuario);