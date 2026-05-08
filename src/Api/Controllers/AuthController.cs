using AgentService.Domain.Dtos.Auth;
using AgentService.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("magic-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestMagicLink(
        [FromBody] MagicLinkRequestDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");

        await authService.RequestMagicLinkAsync(dto.Email, ct);
        return Ok(new { message = "Magic link sent. Check your email." });
    }

    [HttpGet("verify")]
    [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest("Token is required.");

        try
        {
            var jwt = await authService.VerifyMagicLinkAsync(token, ct);
            var expiry = DateTime.UtcNow.AddDays(7);
            return Ok(new { accessToken = jwt, tokenType = "Bearer", expiresAt = expiry });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
