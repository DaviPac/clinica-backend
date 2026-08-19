namespace Clinica.Application.Features.Agendamentos.DTOs;

public class ReagendarAgendamentoRequest
{
    public DateTimeOffset NovoInicio { get; set; }
    public DateTimeOffset NovoFim { get; set; }
    public required bool ReagendarRecorrencia { get; set; }
}