public interface ITextChunker
{
    IReadOnlyList<string> Chunk(string text);
}