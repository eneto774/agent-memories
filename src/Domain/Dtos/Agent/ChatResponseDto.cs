namespace AgentService.Domain.Dtos.Agent;

public record ChatResponseDto(
    string Response,
    Guid ConversationId,
    Guid MessageId,
    int TokensUsed = 0
);
