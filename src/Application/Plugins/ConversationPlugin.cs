using System.ComponentModel;
using AgentService.Domain.Interfaces.Repositories;
using Microsoft.SemanticKernel;

namespace AgentService.Application.Plugins;

public class ConversationPlugin(IConversationRepository conversationRepository, Guid userId)
{
    [KernelFunction("get_conversation_history")]
    [Description("Recupera as ultimas mensagens de uma conversa especifica.")]
    public async Task<string> GetConversationHistoryAsync(
        [Description("O ID da conversa da qual recuperar o historico")] string conversationId,
        [Description("Numero maximo de mensagens a recuperar")] int limit = 10,
        CancellationToken cancellationToken = default
    )
    {
        if (!Guid.TryParse(conversationId, out var convId))
            return "ID de conversa invalido.";

        var conversation = await conversationRepository.GetByIdAsync(convId, cancellationToken);
        if (conversation == null || conversation.UserId != userId)
            return "Conversa nao encontrada.";

        var messages = conversation
            .Messages.OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Reverse()
            .Select(m => $"[{m.Role}]: {m.Content}");

        return string.Join("\n", messages);
    }
}
