namespace Clinica.Application.Features.AcertosComissao.DTOs;

public record CriarAcertoComissaoRequest(
    int ProfissionalId,
    string PeriodoReferencia, // "YYYY-MM"
    decimal ValorPago,
    string? Observacao,
    bool ProfissionalRecebe
);
