using System.Collections.Concurrent;

public class SessionMemoryStore
{
    private readonly ConcurrentDictionary<string, List<Message>> _sessions = new();

    public List<Message> GetSession(string sessionId)
    {
        return _sessions.GetOrAdd(sessionId, _ => new List<Message>());
    }

    public void AddMessage(string sessionId, Message message)
    {
        var session = GetSession(sessionId);
        session.Add(message);
    }

    public void Clear(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }
}