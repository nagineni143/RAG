using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

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

    public Task<object> ExecuteAsync(JsonElement input)
    {
        try
        {
            var expression = input.GetProperty("expression").GetString();

            expression = Regex.Replace(expression, @"[^\d\+\-\*/\(\)]", "");
            var result = new DataTable().Compute(expression, null);

            return Task.FromResult<object>(new
            {
                result = result?.ToString()
            });
        }
        catch
        {
            return Task.FromResult<object>(new
            {
                error = "Invalid mathematical expression."
            });
        }
    }
}