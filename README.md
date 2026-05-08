# Agent Service

API em .NET 9 para chat com autenticacao, historico de conversas e memoria vetorial por usuario usando Qdrant.

## Requisitos

- .NET SDK 9
- Docker e Docker Compose
- Chave da OpenAI

## Como Rodar Localmente

1. Suba as dependencias:

```powershell
docker compose up -d
```

Isso inicia:

- Postgres em `localhost:5432`
- Qdrant HTTP em `localhost:6333`
- Qdrant gRPC em `localhost:6334`

2. Configure os secrets locais da API:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=agent_service;Username=postgres;Password=postgres" --project src\Api\Api.csproj
dotnet user-secrets set "OpenAI:ApiKey" "SUA_CHAVE_OPENAI" --project src\Api\Api.csproj
dotnet user-secrets set "OpenAI:ChatModel" "gpt-4o-mini" --project src\Api\Api.csproj
dotnet user-secrets set "OpenAI:EmbeddingModel" "text-embedding-3-small" --project src\Api\Api.csproj
dotnet user-secrets set "Llm:Provider" "OpenAI" --project src\Api\Api.csproj
dotnet user-secrets set "Qdrant:Host" "localhost" --project src\Api\Api.csproj
dotnet user-secrets set "Qdrant:Port" "6334" --project src\Api\Api.csproj
dotnet user-secrets set "Qdrant:VectorSize" "1536" --project src\Api\Api.csproj
dotnet user-secrets set "Jwt:SecretKey" "troque-por-uma-chave-com-mais-de-32-caracteres" --project src\Api\Api.csproj
```

Se estiver usando o Qdrant local do `docker-compose.yml`, remova qualquer secret local de API key do Qdrant:

```powershell
dotnet user-secrets remove "Qdrant:ApiKey" --project src\Api\Api.csproj
```

3. Restaure e compile:

```powershell
dotnet restore AgentService.sln
dotnet build AgentService.sln
```

4. Rode a API:

```powershell
dotnet run --project src\Api\Api.csproj
```

A API sobe em:

- `http://localhost:5125`
- `https://localhost:7163`

Em ambiente `Development`, a documentacao Scalar fica disponivel em:

```text
http://localhost:5125/scalar/v1
```

## Banco de Dados

As migrations do Entity Framework rodam automaticamente na inicializacao da API.

## Provider de LLM

O provider ativo e definido por:

```text
Llm:Provider
```

Valores aceitos:

- `OpenAI`
- `Ollama`

Para usar OpenAI:

```powershell
dotnet user-secrets set "Llm:Provider" "OpenAI" --project src\Api\Api.csproj
dotnet user-secrets set "OpenAI:ApiKey" "SUA_CHAVE_OPENAI" --project src\Api\Api.csproj
dotnet user-secrets set "OpenAI:ChatModel" "gpt-4o-mini" --project src\Api\Api.csproj
dotnet user-secrets set "OpenAI:EmbeddingModel" "text-embedding-3-small" --project src\Api\Api.csproj
dotnet user-secrets set "Qdrant:VectorSize" "1536" --project src\Api\Api.csproj
```

Para usar Ollama:

```powershell
ollama pull llama3.1
ollama pull nomic-embed-text
dotnet user-secrets set "Llm:Provider" "Ollama" --project src\Api\Api.csproj
dotnet user-secrets set "Ollama:BaseUrl" "http://localhost:11434/v1" --project src\Api\Api.csproj
dotnet user-secrets set "Ollama:ChatModel" "llama3.1" --project src\Api\Api.csproj
dotnet user-secrets set "Ollama:EmbeddingModel" "nomic-embed-text" --project src\Api\Api.csproj
dotnet user-secrets set "Qdrant:VectorSize" "768" --project src\Api\Api.csproj
```

Se trocar o modelo de embedding, ajuste `Qdrant:VectorSize` para a dimensao do embedding do modelo. Colecoes ja criadas no Qdrant mantem o tamanho antigo; para trocar a dimensao, use uma nova colecao ou limpe os dados locais do Qdrant.

## Memoria Vetorial

As memorias do usuario sao salvas no Qdrant em colecoes separadas por usuario:

```text
user_memories_{userId}
```

Tipos usados atualmente:

- `fact`
- `preference`
- `context`
- `semantic`

O tipo `semantic` salva conceitos e explicacoes ja respondidos para reutilizacao em perguntas futuras.

## Observacoes

- Nao commite chaves reais em `appsettings.json`.
- Se o build falhar por arquivos bloqueados em `src\Api\bin`, pare a API/debugger em execucao e rode o build novamente.
