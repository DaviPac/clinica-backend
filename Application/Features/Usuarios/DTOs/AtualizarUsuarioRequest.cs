namespace Clinica.Application.Features.Usuarios.DTOs;

public record AtualizarUsuarioRequest(
    string? Nome,
    string? Email,
    string? Role,
    string? Profissao,
    decimal? TaxaComissaoPadrao,
    bool? ProfissionalRecebe
);