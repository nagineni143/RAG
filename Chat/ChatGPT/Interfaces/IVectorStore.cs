public interface IVectorStore
{
    Task AddAsync(VectorDocument document);

    Task<IReadOnlyList<VectorSearchResult>>
        SearchAsync(string query, IReadOnlyList<float> queryEmbedding, int topK);
}