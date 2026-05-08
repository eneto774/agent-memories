using System.Security.Claims;
using AgentService.Domain.Dtos.Agent;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentService.Api.Controllers;

[ApiController]
[Route("api/agent")]
[Authorize]
public class AgentController(
    IAgentChatService agentChatService,
    IConversationRepository conversationRepository) : ControllerBase
{
    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto dto, CancellationToken ct)
    {
        var userId = GetUserId();
        var response = await agentChatService.ChatAsync(userId, dto, ct);
        return Ok(response);
    }

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(IEnumerable<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var conversations = await conversationRepository.GetByUserIdAsync(userId, limit, ct);
        var dtos = conversations.Select(c => new ConversationDto(c.Id, c.Title, c.CreatedAt, c.UpdatedAt));
        return Ok(dtos);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var conversation = await conversationRepository.GetByIdAsync(conversationId, ct);

        if (conversation == null || conversation.UserId != userId)
            return NotFound();

        var messages = await conversationRepository.GetMessagesAsync(conversationId, limit, ct);
        var dtos = messages.Select(m => new MessageDto(m.Id, m.Role, m.Content, m.CreatedAt));
        return Ok(dtos);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);
}
