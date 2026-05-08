#pragma warning disable SKEXP0010
using System.Globalization;
using System.Text;
using AgentService.Domain.Dtos.Agent;
using AgentService.Domain.Entities;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AgentService.Application.Services;

public class AgentChatService(
    IKernelFactory kernelFactory,
    IConversationRepository conversationRepository,
    ILogger<AgentChatService> logger
) : IAgentChatService
{
    private const string SystemPrompt = """
        Voce e um assistente de IA util com memoria persistente sobre o usuario.

        Use a funcao search_memories para recuperar informacoes relevantes e conceitos ja respondidos antes de responder.
        Quando aprender fatos, preferencias ou contexto importantes sobre o usuario, salve usando save_memory.
        Quando responder um conceito ou explicacao que o usuario possa perguntar novamente, salve um resumo conciso usando save_memory com memoryType "semantic".

        Seja sempre conciso, util e consciente do contexto. Mantenha continuidade ao longo da conversa.
        """;

    public async Task<ChatResponseDto> ChatAsync(
        Guid userId,
        ChatRequestDto request,
        CancellationToken ct = default
    )
    {
        Conversation conversation;
        if (request.ConversationId.HasValue)
        {
            conversation =
                await conversationRepository.GetByIdAsync(request.ConversationId.Value, ct)
                ?? throw new InvalidOperationException("Conversa nao encontrada.");

            if (conversation.UserId != userId)
                throw new UnauthorizedAccessException("A conversa nao pertence a este usuario.");
        }
        else
        {
            conversation = await conversationRepository.CreateAsync(
                new Conversation
                {
                    UserId = userId,
                    Title =
                        request.Message.Length <= 60
                            ? request.Message
                            : request.Message[..57] + "...",
                },
                ct
            );
        }

        var kernel = kernelFactory.CreateForUser(userId);

        var chatHistory = new ChatHistory(request.SystemPrompt ?? SystemPrompt);
        var storedMessages = await conversationRepository.GetMessagesAsync(
            conversation.Id,
            limit: 30,
            ct: ct
        );

        foreach (var msg in storedMessages)
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else
                chatHistory.AddAssistantMessage(msg.Content);
        }

        chatHistory.AddUserMessage(request.Message);

        await conversationRepository.AddMessageAsync(
            new Message
            {
                ConversationId = conversation.Id,
                Role = "user",
                Content = request.Message,
            },
            ct
        );

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            MaxTokens = 2048,
            Temperature = 0.7,
        };

        var result = await chat.GetChatMessageContentAsync(chatHistory, settings, kernel, ct);
        var responseText = result.Content ?? string.Empty;

        var assistantMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = responseText,
        };
        await conversationRepository.AddMessageAsync(assistantMessage, ct);
        await conversationRepository.TouchUpdatedAtAsync(conversation.Id, ct);

        if (ShouldSaveSemanticMemory(request.Message, responseText))
        {
            await kernel.InvokeAsync(
                "Memory",
                "save_memory",
                new KernelArguments
                {
                    ["content"] = BuildSemanticMemoryContent(request.Message, responseText),
                    ["memoryType"] = "semantic",
                },
                ct
            );
        }

        logger.LogInformation(
            "Chat completed for user {UserId}, conversation {ConversationId}",
            userId,
            conversation.Id
        );

        return new ChatResponseDto(responseText, conversation.Id, assistantMessage.Id);
    }

    private static bool ShouldSaveSemanticMemory(string userMessage, string responseText)
    {
        if (string.IsNullOrWhiteSpace(userMessage) || string.IsNullOrWhiteSpace(responseText))
            return false;

        var normalized = NormalizeForSemanticMatch(userMessage);
        return normalized.Contains("conceito")
            || normalized.Contains("o que e")
            || normalized.Contains("o que sao")
            || normalized.Contains("explique")
            || normalized.Contains("explica")
            || normalized.Contains("defina")
            || normalized.Contains("definicao")
            || normalized.Contains("significa");
    }

    private static string NormalizeForSemanticMatch(string value)
    {
        var normalized = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string BuildSemanticMemoryContent(string userMessage, string responseText)
    {
        return $"""
            Pergunta do usuario: {userMessage}

            Conceito respondido: {responseText}
            """;
    }
}
#pragma warning restore SKEXP0010
