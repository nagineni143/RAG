using System.Data;
using System.Text.Json;

public class CalculatorTool : ITool
{
    public string Name => "calculator";

    public string Description =>
        "Evaluates mathematical expressions.";

    public object GetSchema()
    {
        return new
        {
            expression = "string (mathematical expression like 25 * 4)"
        };
    }

    public Task<string> ExecuteAsync(JsonElement input)
    {
        try
        {
            var query = input.GetProperty("expression").GetString();
            var result = new DataTable().Compute(query, null);

            return Task.FromResult(result.ToString() ?? "0");
        }
        catch
        {
            return Task.FromResult("Invalid mathematical expression.");
        }
    }
}