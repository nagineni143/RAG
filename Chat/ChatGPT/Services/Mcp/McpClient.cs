using System.Text.Json;

public class McpClient
{
    private readonly HttpClient _http;
    public McpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<object> ExecuteTool(string toolName, object input)
    {
        var response = await _http.PostAsJsonAsync(
            $"/mcp/tools/{toolName}",
            input);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        return result;
    }
}