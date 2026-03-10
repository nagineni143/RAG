public interface ILLMClient
{
    Task<string> GetResponseAsync(List<Message> messages, double temperature);
}