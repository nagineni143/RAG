using System.Collections.Concurrent;

public class ConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<string, List<Message>> _store = new();

    public Task AddMessageAsync(string sessionId, Message message)
    {
        var list = _store.GetOrAdd(sessionId, _ => new List<Message>());

        lock (list) 
        {
            list.Add(message);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Message>> GetHistoryAsync(string sessionId)
    {
        if (_store.TryGetValue(sessionId, out var list))
        {
            lock (list)
            {
                return Task.FromResult((IReadOnlyList<Message>)list.ToList());
            }
        }

        return Task.FromResult((IReadOnlyList<Message>)[]);
    }
}