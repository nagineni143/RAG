public class VectorRetriever : IRetriever
{
    private readonly IEmbeddingClient _embeddingClient;
    private readonly IVectorStore _vectorStore;
    private readonly IReRanker _reRanker;

    public VectorRetriever(
        IEmbeddingClient embeddingClient,
        IVectorStore vectorStore,
        IReRanker reRanker)
    {
        _embeddingClient = embeddingClient;
        _vectorStore = vectorStore;
        _reRanker = reRanker;
    }

    public async Task<IReadOnlyList<VectorSearchResult>> RetrieveAsync(string query)
    {
        var queryEmbedding =
            await _embeddingClient.GenerateEmbeddingAsync(query);

        var results =
            await _vectorStore.SearchAsync(query, queryEmbedding, topK: 5);

        var reranked = await _reRanker.ReRankAsync(query, results, topK: 3);

        return reranked;
    }
}