using System.Text.Json.Serialization;

namespace Clinica.Application.Features.AcertosComissao.DTOs;

public record AcertoComissaoResponse(
    int Id,
    int ProfissionalId,
    string PeriodoReferencia,
    decimal ValorPago,
    DateTimeOffset DataPagamento,
    string? Observacao,
    bool ProfissionalRecebe
);
