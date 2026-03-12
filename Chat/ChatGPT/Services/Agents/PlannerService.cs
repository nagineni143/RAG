using System.Text.Json;

public class PlannerService
{
    private readonly ILLMClient _llm;
    public PlannerService(ILLMClient llm)
    {
        _llm = llm;
    }
    public async Task<Plan> CreatePlan(string userPrompt)
    {
        var messages = new List<Message>
        {
            new Message
            {
                Role = "system",
                Content = """
                You are an AI planner.
                Break the user's request into steps using available tools.

                Respond ONLY in JSON:

                {
                     "steps": [
                        "step 1 description",
                        "step 2 description"
                    ]
                }
            """
            },
            new Message
            {
                Role = "user",
                Content = userPrompt
            }
        };

        var response = await _llm.GetResponseAsync(messages, 0);

        try
        {
            return JsonSerializer.Deserialize<Plan>(
                response,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            return null;
        }
    }
}