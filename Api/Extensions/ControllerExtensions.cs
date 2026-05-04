using Clinica.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Extensions;

public static class ControllerExtensions
{
    public static ObjectResult HandleError(this ControllerBase controller, Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound     => controller.NotFound(new { error = error.Description, code = error.Id }),
            ErrorType.Unauthorized => controller.Unauthorized(new { error = error.Description, code = error.Id }),
            ErrorType.Conflict     => controller.Conflict(new { error = error.Description, code = error.Id }),
            ErrorType.Validation   => controller.BadRequest(new { error = error.Description, code = error.Id }),
            _                      => controller.StatusCode(500, new { error = "Erro interno", code = "InternalError" })
        };
    }
}