#pragma warning disable SKEXP0001, SKEXP0010
using System.ClientModel;
using AgentService.Application.Plugins;
using AgentService.Domain.Interfaces.Repositories;
using AgentService.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI;

namespace AgentService.Application.Services;

public class KernelFactory(
    IConfiguration configuration,
    IVectorUserMemoryService vectorMemory,
    IConversationRepository conversationRepository) : IKernelFactory
{
    public Kernel CreateForUser(Guid userId)
    {
        var builder = Kernel.CreateBuilder();
        ConfigureLlmProvider(builder);

        var kernel = builder.Build();

        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        kernel.Plugins.AddFromObject(
            new UserMemoryPlugin(vectorMemory, embeddingService, userId),
            "Memory");

        kernel.Plugins.AddFromObject(
            new ConversationPlugin(conversationRepository, userId),
            "Conversation");

        return kernel;
    }

    private void ConfigureLlmProvider(IKernelBuilder builder)
    {
        var provider = configuration["Llm:Provider"] ?? "OpenAI";

        if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            ConfigureOllama(builder);
            return;
        }

        if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            ConfigureOpenAI(builder);
            return;
        }

        throw new InvalidOperationException(
            $"Provider de LLM invalido: '{provider}'. Use 'OpenAI' ou 'Ollama'."
        );
    }

    private void ConfigureOpenAI(IKernelBuilder builder)
    {
        var apiKey = configuration["OpenAI:ApiKey"]!;
        var chatModel = configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";
        var embeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

        builder.AddOpenAIChatCompletion(chatModel, apiKey);
        builder.AddOpenAITextEmbeddingGeneration(embeddingModel, apiKey);
    }

    private void ConfigureOllama(IKernelBuilder builder)
    {
        var baseUrl = NormalizeOllamaBaseUrl(
            configuration["Ollama:BaseUrl"] ?? "http://localhost:11434/v1"
        );
        var chatModel = configuration["Ollama:ChatModel"] ?? "llama3.1";
        var embeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        var apiKey = configuration["Ollama:ApiKey"] ?? "ollama";

        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(baseUrl) }
        );

        builder.AddOpenAIChatCompletion(chatModel, client);
        builder.AddOpenAITextEmbeddingGeneration(embeddingModel, client);
    }

    private static string NormalizeOllamaBaseUrl(string baseUrl)
    {
        var normalized = baseUrl.Trim().TrimEnd('/');
        return normalized.EndsWith("/v1", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"{normalized}/v1";
    }
}
#pragma warning restore SKEXP0001, SKEXP0010
