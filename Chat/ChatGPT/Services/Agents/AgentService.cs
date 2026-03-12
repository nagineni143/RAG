using System.Text.Json;

public class AgentService
{
    private readonly ILLMClient _llm;
    private readonly ToolRegistry _toolRegistry;
    private readonly PlannerService _planner;

    private readonly AgentMemory _memory = new();
    private readonly SessionMemoryStore _sessionStore;

    public AgentService(ILLMClient llm, ToolRegistry toolRegistry, PlannerService plannerService, SessionMemoryStore sessionMemory)
    {
        _llm = llm;
        _toolRegistry = toolRegistry;
        _planner = plannerService;
        _sessionStore = sessionMemory;
    }

    public async Task<string> RunAsync(string sessionId, string userPrompt)
    {
        _memory.Clear();
        var tools = _toolRegistry.GetAll();

        var toolSchemas = string.Join("\n\n", tools.Select(t =>
            $"{t.Name}:\nDescription: {t.Description}\nSchema: {JsonSerializer.Serialize(t.GetSchema())}"));

        var history = _sessionStore.GetSession(sessionId);
        var messages = new List<Message>
        {
            new Message
            {
                Role = "system",
                Content = """
                You are an AI agent that can use tools to answer questions.

                Thought: decide what to do
                Action: call a tool
                Observation: read the tool result
                Repeat until you can answer.

                Available tools:
                """ + toolSchemas + """

                When a tool is needed, respond ONLY with JSON in this format:
                { "tool": "tool_name", "input": "tool input" }
                When you have enough information to answer the user, respond with:
                { "tool": "none", "final_answer": "your answer" }

                Rules:
                - Do not include explanations outside the JSON.
                - Do not include markdown.
                - Always return valid JSON.

                Use tools when external information or computation is required.
                """
            }
        };

        messages.AddRange(history);
        messages.Add(new Message
        {
            Role = "user",
            Content = userPrompt
        }
        );
        _sessionStore.AddMessage(sessionId, new Message
        {
            Role = "user",
            Content = userPrompt
        });

        var plan = await _planner.CreatePlan(userPrompt);
        if (plan?.Steps?.Any() == true)
        {
            var planText = string.Join("\n", plan.Steps);

            messages.Add(new Message
            {
                Role = "system",
                Content = $"""
                        Execution plan:

                        {planText}

                        Follow this plan step-by-step when deciding which tools to use.
                        """
            });
        }
        for (int step = 0; step < 5; step++)
        {
            var memoryText = _memory.GetMemory();

            if (!string.IsNullOrWhiteSpace(memoryText))
            {
                messages.Add(new Message
                {
                    Role = "system",
                    Content = $"Previous observations:\n{memoryText}"
                });
            }
            var response = await _llm.GetResponseAsync(messages, 0);

            Console.WriteLine("Planner raw response:");
            Console.WriteLine(response);

            ToolCall toolCall;

            try
            {
                toolCall = JsonSerializer.Deserialize<ToolCall>(response);
                if (toolCall == null)
                {
                    Console.WriteLine("Failed to deserialize ToolCall");
                    return "Agent error.";
                }
                var root = JsonDocument.Parse(response).RootElement;
                JsonElement input = toolCall.Input;

                if (input.ValueKind == JsonValueKind.Undefined || input.ValueKind == JsonValueKind.Null)
                {
                    // Handle flattened parameters
                    if (root.TryGetProperty("query", out var query))
                        input = query;

                    if (root.TryGetProperty("expression", out var expression))
                        input = expression;
                }

                if (toolCall == null)
                    return "Agent error.";

                if (toolCall.Tool == "none")
                {
                    _sessionStore.AddMessage(sessionId, new Message
                    {
                        Role = "assistant",
                        Content = toolCall.FinalAnswer
                    });
                    return toolCall.FinalAnswer ?? "No answer.";
                }

                var tool = _toolRegistry.Get(toolCall.Tool);

                if (tool == null)
                    return "Unknown tool.";

                var toolResult = await tool.ExecuteAsync(input);
                _memory.AddObservation(toolResult);

                messages.Add(new Message
                {
                    Role = "assistant",
                    Content = response
                });

                messages.Add(new Message
                {
                    Role = "system",
                    Content = $"Tool result:\n{toolResult}"
                });
            }
            catch
            {
                return "Agent failed to parse response.";
            }

        }

        return "Agent reached step limit.";

    }
}