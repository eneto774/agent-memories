namespace AgentService.Domain.Dtos.Agent;

public record ChatRequestDto(
    string Message,
    Guid? ConversationId = null,
    string? SystemPrompt = null
);
