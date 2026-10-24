using System.Text.Json;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record ProntuarioVersaoResponse(
    int VersaoId,
    JsonElement Conteudo,
    int AutorId,
    string? Autor,
    DateTime CriadoEm
);
