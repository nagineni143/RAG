public interface IConversationStore
{
    Task<IReadOnlyList<Message>> GetHistoryAsync(string sessionId);
    Task AddMessageAsync(string sessionId, Message message);
}
