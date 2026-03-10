public interface IEmbeddingClient
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text);
}