public interface IRetriever
{
    Task<IReadOnlyList<VectorSearchResult>> RetrieveAsync(string query);
}