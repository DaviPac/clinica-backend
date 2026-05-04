using Clinica.Domain.Enums;
using System.Security.Claims;

namespace Clinica.Api.Extensions;

public static class ClaimsExtensions
{
    public static int GetUserId(this HttpContext ctx)
    {
        var claim = ctx.User.FindFirstValue("user_id");
        return int.TryParse(claim, out var id) ? id : 0;
    }

    public static Role GetRole(this HttpContext ctx)
    {
        var claim = ctx.User.FindFirstValue("role");
        return Enum.TryParse<Role>(claim, out var role) ? role : Role.PROFISSIONAL;
    }
}