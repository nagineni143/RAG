using System.Text.Json;
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
builder.Services.AddSingleton<McpServer>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddSingleton<IMemoryManager, SlidingWindowMemoryManager>();
builder.Services.AddSingleton<IConversationStore, ConversationStore>();
builder.Services.AddSingleton<ITextChunker, OverlappingTextChunker>();

builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddHttpClient<McpClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5208");
});

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

app.MapPost("/mcp/tools/{toolName}", async (
    string toolName,
    JsonElement body,
    ToolRegistry registry) =>
{
    var tool = registry.Get(toolName);

    if (tool == null)
        return Results.BadRequest($"Tool '{toolName}' not found.");

    var result = await tool.ExecuteAsync(body);

    return Results.Ok(new
    {
        result
    });
})
.WithName("ToolExecution");

app.MapGet("/mcp/tools", (ToolRegistry registry) =>
{
    var tools = registry.GetAll();

    return tools.Select(t => new
    {
        name = t.Name,
        description = t.Description,
        input_schema = t.GetSchema()
    });
})
.WithName("Tools");

app.MapPost("/mcp", async (JsonElement body, ToolRegistry registry) =>
{
    var method = body.GetProperty("method").GetString();

    if (method == "tools/list")
    {
        var tools = registry.GetAll();

        return Results.Ok(new
        {
            tools = tools.Select(t => new
            {
                name = t.Name,
                description = t.Description,
                input_schema = t.GetSchema()
            })
        });
    }

    if (method == "tools/call")
    {
        var name = body.GetProperty("params").GetProperty("name").GetString();
        var args = body.GetProperty("params").GetProperty("arguments");

        var tool = registry.Get(name);

        if (tool == null)
            return Results.BadRequest("Tool not found");

        var result = await tool.ExecuteAsync(args);

        return Results.Ok(new { result });
    }

    return Results.BadRequest("Unknown MCP method");
})
.WithName("MCPSPEC");

app.Run();
