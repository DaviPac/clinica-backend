using System.Text.Json.Serialization;

namespace Clinica.Domain;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string SenhaHash { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; }

    public string? Profissao { get; set; }
    [JsonPropertyName("taxa_comissao_padrao")]
    public decimal TaxaComissaoPadrao { get; set; }
    [JsonPropertyName("criado_em")]
    public DateTime CriadoEm { get; set; }

    [JsonIgnore]
    public ICollection<PacienteProfissional> PacientesAtendidos { get; set; } = [];
    [JsonIgnore]
    public ICollection<Servico> Servicos { get; set; } = [];
}