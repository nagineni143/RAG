using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<OpenAIInfrastructure>();

builder.Services.AddSingleton<ILLMClient, OpenAIApiClient>();
builder.Services.AddSingleton<IEmbeddingClient, OpenAiEmbeddingClient>();
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddSingleton<IReRanker, SimpleReRanker>();
builder.Services.AddSingleton<IRetriever, VectorRetriever>();
builder.Services.AddSingleton<ITool, RagSearchTool>();
builder.Services.AddSingleton<ITool, CalculatorTool>();
builder.Services.AddScoped<PlannerService>();
builder.Services.AddSingleton<SessionMemoryStore>();
builder.Services.AddSingleton<ToolRegistry>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddSingleton<IMemoryManager, SlidingWindowMemoryManager>();
builder.Services.AddSingleton<IConversationStore, ConversationStore>();
builder.Services.AddSingleton<ITextChunker, OverlappingTextChunker>();



builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Resolve services
using (var scope = app.Services.CreateScope())
{
    var embeddingClient = scope.ServiceProvider.GetRequiredService<IEmbeddingClient>();
    var vectorStore = scope.ServiceProvider.GetRequiredService<IVectorStore>();
    var chunker = scope.ServiceProvider.GetRequiredService<ITextChunker>();

    var text = await File.ReadAllTextAsync("Data/design_patterns.txt");

    var chunks = chunker.Chunk(text);
    int chunkIndex = 0;
    foreach (var chunk in chunks)
    {
        var embedding = await embeddingClient.GenerateEmbeddingAsync(chunk);

        await vectorStore.AddAsync(new VectorDocument
        {
            DocumentId = "design_patterns_doc",
            ChunkIndex = chunkIndex++,
            Content = chunk,
            Embedding = embedding
        });
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapPost("/chat", async (Request request, IChatService _chatService) =>
{
    return await _chatService.ChatAsync(request.SessionId, request.Message);
})
.WithName("Chat");

app.MapPost("/agent", async (AgentService agent, string sessionId, UserRequest request) =>
{
    return await agent.RunAsync(sessionId, request.Message);
})
.WithName("Agent");

app.Run();
