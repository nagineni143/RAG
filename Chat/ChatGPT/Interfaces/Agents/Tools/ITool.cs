using System.Text.Json;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    object GetSchema();
    Task<object> ExecuteAsync(JsonElement input);
}