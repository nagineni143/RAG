public interface IReRanker
{
    Task<IReadOnlyList<VectorSearchResult>> ReRankAsync(
        string query,
        IReadOnlyList<VectorSearchResult> candidates,
        int topK);
}