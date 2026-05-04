using Clinica.Domain.Enums;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record AtualizarStatusAgendamentoRequest(
    StatusAgendamento Status
);