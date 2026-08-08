using Clinica.Domain.Enums;

namespace Clinica.Domain.ReadModels;

public record RelatorioSessoes(
    int ProfissionalId,
    string NomeProfissional,
    DateOnly Inicio,
    DateOnly Fim,
    IReadOnlyList<SessaoRelatorio> Sessoes,
    TotaisRelatorioSessoes Totais
);

public record SessaoRelatorio(
    int AgendamentoId,
    DateTimeOffset DataHoraInicio,
    string NomePaciente,
    string NomeServico,
    StatusAgendamento Status,
    bool PagoPeloPaciente,
    bool ProfissionalRecebe,
    decimal ValorSessao,
    decimal PercentualComissao,
    decimal ParteClinica,
    decimal ParteProfissional,
    decimal RecebidoPelaClinica,
    decimal RecebidoPeloProfissional,
    decimal DevidoAoProfissional,
    decimal DevidoAClinica
);

public record TotaisRelatorioSessoes(
    int QuantidadeSessoes,
    decimal ValorTotal,
    decimal RecebidoPelaClinica,
    decimal RecebidoPeloProfissional,
    decimal DevidoAoProfissional,
    decimal DevidoAClinica
);
