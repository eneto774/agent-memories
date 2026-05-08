#pragma warning disable SKEXP0001
using System.ComponentModel;
using AgentService.Domain.Interfaces.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace AgentService.Application.Plugins;

public class UserMemoryPlugin(
    IVectorUserMemoryService vectorMemory,
    ITextEmbeddingGenerationService embeddingService,
    Guid userId
)
{
    [KernelFunction("search_memories")]
    [Description(
        "Busca informacoes relevantes na memoria pessoal do usuario com base em uma consulta."
    )]
    public async Task<string> SearchMemoriesAsync(
        [Description("O que buscar na memoria do usuario")] string query,
        CancellationToken cancellationToken = default
    )
    {
        var vector = await embeddingService.GenerateEmbeddingAsync(
            query,
            cancellationToken: cancellationToken
        );
        var results = await vectorMemory.SearchMemoriesByVectorAsync(
            userId,
            vector.ToArray(),
            limit: 5,
            ct: cancellationToken
        );

        var list = results.ToList();
        return list.Count == 0
            ? "Nenhuma memoria relevante encontrada."
            : string.Join("\n- ", list.Prepend("Memorias relevantes:"));
    }

    [KernelFunction("save_memory")]
    [Description(
        "Salva um fato, preferencia, contexto ou conceito respondido importante para referencia futura."
    )]
    public async Task<string> SaveMemoryAsync(
        [Description("A informacao a ser lembrada sobre o usuario")] string content,
        [Description("O tipo da memoria: 'fact', 'preference', 'context' ou 'semantic'")]
            string memoryType = "fact",
        CancellationToken cancellationToken = default
    )
    {
        var vector = await embeddingService.GenerateEmbeddingAsync(
            content,
            cancellationToken: cancellationToken
        );
        var memoryId = Guid.NewGuid();
        await vectorMemory.UpsertMemoryVectorAsync(
            userId,
            memoryId,
            vector.ToArray(),
            content,
            memoryType,
            cancellationToken
        );
        return $"Memoria salva: {content}";
    }
}
#pragma warning restore SKEXP0001
