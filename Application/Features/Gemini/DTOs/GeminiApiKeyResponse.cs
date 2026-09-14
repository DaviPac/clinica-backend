using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Gemini.DTOs;

public record GeminiApiKeyResponse(
    [property: JsonPropertyName("api_key")]
    string ApiKey
);
