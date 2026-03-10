using System.Text.Json;

public class AgentService
{
    private readonly ILLMClient _llm;
    private readonly ToolRegistry _toolRegistry;

    public AgentService(ILLMClient llm, ToolRegistry toolRegistry)
    {
        _llm = llm;
        _toolRegistry = toolRegistry;
    }

    public async Task<string> RunAsync(string userPrompt)
    {
        var tools = _toolRegistry.GetAll();

        var toolDescriptions = string.Join("\n",
            tools.Select(t => $"{t.Name}: {t.Description}"));

        var messages = new List<Message>
        {
            new Message
            {
                Role = "system",
                Content = """
                You are an AI agent that answers questions using tools.
                You MUST use a tool if the question asks about information
                that may exist in internal documents.
                Available tools:
                """ + toolDescriptions + """
                When a tool is required, respond ONLY with JSON in this format:
                { "tool": "tool_name", "input": "user request or refined query" }
                If no tool is needed, respond:
                { "tool": "none", "final_answer": "your answer"}
                Rules:
                - If the question asks about design patterns, internal knowledge, or documentation,
                  you MUST use the rag_search tool.
                - Do not answer the question yourself.

                Example:
                User: Which design pattern ensures one instance exists?
                { "tool": "rag_search", "input": "singleton design pattern" }
                """
            },

            new Message
            {
                Role = "user",
                Content = userPrompt
            }
        };

        for (int step = 0; step < 5; step++)
        {
            var response = await _llm.GetResponseAsync(messages, 0);

            ToolCall? toolCall;

            try
            {
                toolCall = JsonSerializer.Deserialize<ToolCall>(response);
            }
            catch
            {
                return "Agent failed to parse response.";
            }

            if (toolCall == null)
                return "Agent error.";

            if (toolCall.Tool == "none")
            {
                return toolCall.FinalAnswer ?? "No answer.";
            }

            var tool = _toolRegistry.Get(toolCall.Tool);

            if (tool == null)
                return "Unknown tool.";

            var toolResult = await tool.ExecuteAsync(toolCall.Input);

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

        return "Agent reached step limit.";

    }
}