using System.Text.Json;
using System.Text.Json.Serialization;

public class ToolCall
{
    [JsonPropertyName("tool")]
    public string Tool { get; set; } = "";
    [JsonPropertyName("input")]
    public JsonElement Input { get; set; }
    [JsonPropertyName("final_answer")]
    public string FinalAnswer { get; set; }
}