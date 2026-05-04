using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record AtualizarPagamentoAgendamentoRequest(
    [property: JsonPropertyName("pago_pelo_paciente")]
    bool PagoPeloPaciente
);