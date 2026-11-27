using System.Security.Cryptography;
using System.Text;
using CareEMR.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CareEMR.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokens;
    private readonly DemoUserOptions _demoUser;

    public AuthController(TokenService tokens, IOptions<DemoUserOptions> demoUser)
    {
        _tokens = tokens;
        _demoUser = demoUser.Value;
    }

    // POST /api/auth/login
    // DEMO ONLY: checks against the single demo user from configuration.
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (!SecureEquals(request.Username, _demoUser.Username) ||
            !SecureEquals(request.Password, _demoUser.Password))
        {
            return Problem(detail: "Invalid username or password.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return _tokens.CreateToken(_demoUser.Username);
    }

    // Constant-time comparison, so attackers can't guess the password by measuring response times.
    private static bool SecureEquals(string? a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a ?? string.Empty), Encoding.UTF8.GetBytes(b));
}
