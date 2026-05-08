namespace AgentService.Domain.Dtos.Agent;

public record ConversationDto(Guid Id, string Title, DateTime CreatedAt, DateTime UpdatedAt);

public record MessageDto(Guid Id, string Role, string Content, DateTime CreatedAt);
