public class RagSearchTool : ITool
{
    private readonly IRetriever _retriever;
    public RagSearchTool(IRetriever retriever)
    {
        _retriever = retriever;
    }
    public string Name => "rag_search";
    public string Description => "Searches internal documents for relevant information.";
    public async Task<string> ExecuteAsync(string input)
    {
        var docs = await _retriever.RetrieveAsync(input);

        if (!docs.Any())
            return "No relevant documents found.";

        return string.Join("\n\n", docs.Select(d => d.Content));
    }
}