namespace Clinica.Application.Features.Gemini;

/// <summary>
/// Configuração da integração com o Gemini.
/// Preenchida pela seção "Gemini" do appsettings ou, em produção, pela variável
/// de ambiente Gemini__ApiKey (o duplo sublinhado é o separador de seção que o
/// .NET reconhece — é assim que se define no Railway).
/// </summary>
public sealed class GeminiOptions
{
    public const string Secao = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
}
