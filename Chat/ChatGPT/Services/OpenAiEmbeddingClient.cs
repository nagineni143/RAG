using OpenAI.Embeddings;

public class OpenAiEmbeddingClient : IEmbeddingClient
{
    private readonly EmbeddingClient _embeddingClient;

    public OpenAiEmbeddingClient(OpenAIInfrastructure infra)
    {
        _embeddingClient = infra.Client.GetEmbeddingClient("text-embedding-3-small");
    }

    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text)
    {
        var response = await _embeddingClient.GenerateEmbeddingAsync(text);
        return response.Value.ToFloats().ToArray();
    }
}