using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Usuarios.DTOs;

public record UsuarioResponse(
    int Id,
    string Nome,
    string Email,
    string Role,
    string? Profissao,
    decimal TaxaComissaoPadrao,
    DateTime CriadoEm,
    bool ProfissionalRecebe
);