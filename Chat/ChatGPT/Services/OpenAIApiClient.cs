using OpenAI;
using OpenAI.Chat;

public class OpenAIApiClient : ILLMClient
{
    private readonly ChatClient _chatClient;

    public OpenAIApiClient(OpenAIInfrastructure infra)
    {
        _chatClient = infra.Client.GetChatClient("gpt-4o-mini");
    }

    public async Task<string> GetResponseAsync(List<Message> messages, double temperature)
    {
        var chatMessages = messages.Select<Message, ChatMessage>(m =>
            m.Role switch
            {
                "system" => new SystemChatMessage(m.Content),
                "assistant" => new AssistantChatMessage(m.Content),
                _ => new UserChatMessage(m.Content)
            }).ToList();

        var response = await _chatClient.CompleteChatAsync(chatMessages);

        return response.Value.Content[0].Text;
    }
}