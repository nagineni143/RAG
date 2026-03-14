public class ChatService : IChatService
{
    private readonly IConversationStore _store;
    private readonly ILLMClient _llm;
    private readonly IMemoryManager _memoryBuilder;
    private readonly IRetriever _retriever;

    private const string SystemPrompt =
        "You are a senior .NET architect who gives concise and accurate answers.";

    public ChatService(IConversationStore store, ILLMClient llmClient, IMemoryManager memoryBuilder, IRetriever retriever)
    {
        _store = store;
        _llm = llmClient;
        _memoryBuilder = memoryBuilder;
        _retriever = retriever;
    }

    public async Task<string> ChatAsync(string sessionId, string userPrompt)
    {
        // 1. Load full history
        var history = await _store.GetHistoryAsync(sessionId);

        var userMessage = new Message
        {
            Role = "user",
            Content = userPrompt
        };

        // 2. Ask memory manager to prepare conversation memory
        var conversationMemory =
        _memoryBuilder.BuildPromptMemory(history, userMessage);

        // 3. Compose final prompt with system message
        var retrievedDocs = await _retriever.RetrieveAsync(userPrompt)b;
        string contextBlock;
        if (!retrievedDocs.Any())
        {
            contextBlock = "No relevant documents found.";
        }
        else
        {
            contextBlock = string.Join("\n\n", retrievedDocs.Select(d =>
                $"Source: {d.DocumentId} (chunk {d.ChunkIndex})\n{d.Content}"));
        }

        var finalPrompt = new List<Message>
        {
            new Message { Role = "system", Content = SystemPrompt },
            new Message
            {
                Role = "system",
                Content = $"""
                Answer the user's question ONLY using the provided context.
                Always include the source and chunkId after the answer.
                If the answer cannot be found in the context,
                respond with: "I don't have enough information."

                Context:
                {contextBlock}
                """
            }
        };

        finalPrompt.AddRange(conversationMemory);

        // 4. Call LLM
        var response = await _llm.GetResponseAsync(finalPrompt, temperature: 0.7);

        // 5. Persist after success
        await _store.AddMessageAsync(sessionId, userMessage);

        await _store.AddMessageAsync(sessionId, new Message
        {
            Role = "assistant",
            Content = response
        });

        return response;
    }
}