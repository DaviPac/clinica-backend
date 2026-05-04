using System.Text.Json.Serialization;

namespace Clinica.Domain.ReadModels;

public record RelatorioFinanceiro(
    string Periodo,
    IReadOnlyList<ResumoComissaoProfissional> Profissionais,
    [property: JsonPropertyName("total_comissoes")]
    decimal TotalComissoes,
    [property: JsonPropertyName("total_despesas")]
    decimal TotalDespesas,
    [property: JsonPropertyName("lucro_liquido")]
    decimal LucroLiquido
);

public record ResumoComissaoProfissional(
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId,
    [property: JsonPropertyName("nome_profissional")]
    string NomeProfissional,
    [property: JsonPropertyName("total_recebido")]
    decimal TotalRecebido, // soma dos valores_combinados (tudo entra na clínica)
    [property: JsonPropertyName("comissao_clinica")]
    decimal ComissaoClinica, // parte que fica na clínica
    [property: JsonPropertyName("a_receber")]
    decimal AReceber, // parte bruta devida ao profissional
    [property: JsonPropertyName("total_repassado")]
    decimal TotalRepassado, // já pago via acerto
    decimal Pendente // a_receber - total_repassado
);