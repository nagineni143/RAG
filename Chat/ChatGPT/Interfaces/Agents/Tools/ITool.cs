using System.Text.Json;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    object GetSchema();
    Task<string> ExecuteAsync(JsonElement input);
}