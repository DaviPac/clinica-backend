using System.Text.Json.Serialization;

namespace Clinica.Domain.ReadModels;

public record RelatorioFinanceiro(
    string Periodo,
    IReadOnlyList<ResumoComissaoProfissional> Profissionais,
    decimal TotalComissoes,
    decimal TotalDespesas,
    decimal LucroLiquido
);

public record ResumoComissaoProfissional(
    int ProfissionalId,
    string NomeProfissional,
    decimal TotalFaturado,
    decimal ComissaoClinica,
    decimal DevidoAoProfissional,
    decimal DevidoAClinica,
    decimal RepassadoAoProfissional,
    decimal RepassadoAClinica,
    decimal PendenteAoProfissional,
    decimal PendenteAClinica
);