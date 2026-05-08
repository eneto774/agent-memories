using System.Security.Claims;
using AgentService.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentService.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!
        );
        var user = await userRepository.GetByIdAsync(userId, ct);

        if (user == null)
            return NotFound();

        return Ok(
            new
            {
                user.Id,
                user.Email,
                user.Name,
                user.CreatedAt,
            }
        );
    }

    [HttpPatch("me/name")]
    public async Task<IActionResult> UpdateName(
        [FromBody] UpdateNameRequest req,
        CancellationToken ct
    )
    {
        var userId = Guid.Parse(
            User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!
        );
        var user = await userRepository.GetByIdAsync(userId, ct);

        if (user == null)
            return NotFound();
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required.");

        user.Name = req.Name.Trim();
        await userRepository.UpdateAsync(user, ct);
        return Ok(new { user.Id, user.Name });
    }

    public record UpdateNameRequest(string Name);
}
