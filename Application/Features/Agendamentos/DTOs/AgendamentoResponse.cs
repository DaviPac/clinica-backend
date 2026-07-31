namespace Clinica.Application.Features.Agendamentos.DTOs;

public record AgendamentoResponse(
    int Id,
    int PacienteId,
    int ProfissionalId,
    int ServicoId,
    DateTimeOffset DataHoraInicio,
    DateTimeOffset DataHoraFim,
    decimal ValorCombinado,
    decimal? ValorPacote,
    decimal PercentualComissaoMomento,
    string Status,
    bool PagoPeloPaciente,
    string? RecorrenciaGroupId,
    DateTime CriadoEm,
    bool ProfissionalRecebe
);