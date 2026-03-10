public class OverlappingTextChunker : ITextChunker
{
    private const int ChunkSize = 200;
    private const int OverlapSize = 50;

    public IReadOnlyList<string> Chunk(string text)
    {
        var sentences = text.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var chunks = new List<string>();
        var currentChunk = "";

        foreach (var sentence in sentences)
        {
            if ((currentChunk + sentence).Length > ChunkSize)
            {
                chunks.Add(currentChunk.Trim());
                currentChunk = "";
            }

            currentChunk += sentence + ". ";
        }

        if (!string.IsNullOrWhiteSpace(currentChunk))
            chunks.Add(currentChunk.Trim());

        return chunks;
    }
}