using System.Text.Json;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record SalvarDocumentoRequest(
    JsonElement Conteudo
);
