using Clinica.Application.Features.Gemini;
using Clinica.Application.Features.Gemini.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("gemini")]
public class GeminiController(IOptions<GeminiOptions> options) : ControllerBase
{
    /// <summary>
    /// Entrega ao frontend a chave usada pelo assistente de IA.
    /// </summary>
    /// <remarks>
    /// Exige autenticação: sem isso qualquer um na internet baixaria a chave com
    /// um curl e o consumo cairia na conta da clínica. Ainda assim, a chave chega
    /// ao navegador e um usuário logado consegue extraí-la do DevTools — o
    /// arranjo sem esse risco é a API intermediar as chamadas ao Gemini, em vez
    /// de entregar a chave.
    /// </remarks>
    [HttpGet("api-key")]
    [Authorize]
    public IActionResult ObterApiKey()
    {
        var apiKey = options.Value.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(503, new
            {
                error = "Assistente de IA não configurado nesta instalação.",
                code = "GeminiApiKeyNaoConfigurada"
            });

        return Ok(new GeminiApiKeyResponse(apiKey));
    }
}
