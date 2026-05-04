using Clinica.Domain.Enums;

namespace Clinica.Application.Features.Auth.DTOs;

public record RegistrarUsuarioRequest(
    string  Nome,
    string  Email,
    string  Senha,
    Role?    Role,
    string? Profissao,
    decimal? TaxaComissaoPadrao
);
