namespace Clinica.Application.Features.Agendamentos.DTOs;

public record AgendamentoResponse(
    int Id,
    int PacienteId,
    string? Paciente,
    int ProfissionalId,
    string? Profissional,
    int ServicoId,
    string? Servico,
    DateTimeOffset DataHoraInicio,
    DateTimeOffset DataHoraFim,
    decimal ValorCombinado,
    decimal? ValorPacote,
    decimal PercentualComissaoMomento,
    string Status,
    bool PagoPeloPaciente,
    string? RecorrenciaGroupId,
    DateTime CriadoEm,
    bool ProfissionalRecebe,
    bool PossuiProntuario
);
