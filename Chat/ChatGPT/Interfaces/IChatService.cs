public interface IChatService
{
    Task<string> ChatAsync(string sessionId, string message);
}