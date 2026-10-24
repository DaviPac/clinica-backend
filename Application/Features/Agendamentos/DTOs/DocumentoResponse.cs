using System.Text.Json;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record DocumentoResponse(
    JsonElement Conteudo
);
