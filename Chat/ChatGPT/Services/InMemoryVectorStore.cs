public class InMemoryVectorStore : IVectorStore
{
    private readonly List<VectorDocument> _documents = new();

    public Task AddAsync(VectorDocument document)
    {
        _documents.Add(document);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
    string query,
    IReadOnlyList<float> queryEmbedding,
    int topK)
    {
        var results = _documents
            .Select(doc =>
            {
                var vectorScore = CosineSimilarity(queryEmbedding, doc.Embedding);
                var keywordScore = KeywordScore(query, doc.Content);

                return new VectorSearchResult
                {
                    DocumentId = doc.DocumentId,
                    ChunkIndex = doc.ChunkIndex,
                    Content = doc.Content,
                    VectorScore = vectorScore,
                    KeywordScore = keywordScore,
                    FinalScore = (vectorScore * 0.7) + (keywordScore * 0.3)
                };
            })
            .OrderByDescending(r => r.FinalScore)
            .Take(topK)
            .ToList();

        return Task.FromResult((IReadOnlyList<VectorSearchResult>)results);
    }

    private static double CosineSimilarity(
        IReadOnlyList<float> v1,
        IReadOnlyList<float> v2)
    {
        double dot = 0;
        double mag1 = 0;
        double mag2 = 0;

        for (int i = 0; i < v1.Count; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }

        mag1 = Math.Sqrt(mag1);
        mag2 = Math.Sqrt(mag2);

        return dot / (mag1 * mag2);
    }

    private static double KeywordScore(string query, string content)
    {
        var queryWords = query
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int matches = queryWords.Count(w => content.ToLower().Contains(w));

        return (double)matches / queryWords.Length;
    }
}