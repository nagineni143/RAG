public class SimpleReRanker : IReRanker
{
    public Task<IReadOnlyList<VectorSearchResult>> ReRankAsync(
        string query,
        IReadOnlyList<VectorSearchResult> candidates,
        int topK)
    {
        var ranked = candidates
            .OrderByDescending(c => c.FinalScore)
            .Take(topK)
            .ToList();

        return Task.FromResult(
            (IReadOnlyList<VectorSearchResult>)ranked);
    }
}