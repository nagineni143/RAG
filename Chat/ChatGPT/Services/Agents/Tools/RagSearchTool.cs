using System.Text.Json;

public class RagSearchTool : ITool
{
    private readonly IRetriever _retriever;
    public RagSearchTool(IRetriever retriever)
    {
        _retriever = retriever;
    }
    public string Name => "rag_search";
    public string Description => "Searches internal documents for relevant information.";

    public object GetSchema()
    {
        return new
        {
            query = "string (search query for internal documents)"
        };
    }
    public async Task<object> ExecuteAsync(JsonElement input)
    {
        string query = "";

        if (input.ValueKind == JsonValueKind.String)
            query = input.GetString();

        else if (input.ValueKind == JsonValueKind.Object &&
                 input.TryGetProperty("query", out var q))
            query = q.GetString();

        var docs = await _retriever.RetrieveAsync(query);

        return new
        {
            results = docs.Select(d => d.Content)
        };
    }
}