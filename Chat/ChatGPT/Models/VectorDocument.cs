public class VectorDocument
{
    public string DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; }
    public IReadOnlyList<float> Embedding { get; set; }
}