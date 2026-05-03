using System.Text.Json.Serialization;

namespace Api.Domain;

public class Servico
{
    public int Id { get; set; }
    [JsonPropertyName("profissional_id")]
    public int ProfissionalId { get; set; }
    [JsonIgnore]
    public required Usuario Profissional { get; set; }
    public string Nome { get; set; } = string.Empty;
    [JsonPropertyName("valor_atual")]
    public decimal ValorAtual { get; set; }
    public bool Ativo { get; set; } = true;
    [JsonPropertyName("is_pacote")]
    public bool IsPacote { get; set; } = false;
}