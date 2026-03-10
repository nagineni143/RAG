public class VectorSearchResult
{
    public string DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; }

    public double VectorScore { get; set; }
    public double KeywordScore { get; set; }

    public double FinalScore { get; set; }
}