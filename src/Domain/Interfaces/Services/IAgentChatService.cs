using AgentService.Domain.Dtos.Agent;

namespace AgentService.Domain.Interfaces.Services;

public interface IAgentChatService
{
    Task<ChatResponseDto> ChatAsync(Guid userId, ChatRequestDto request, CancellationToken ct = default);
}
