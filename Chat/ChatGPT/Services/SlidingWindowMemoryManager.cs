public class SlidingWindowMemoryManager : IMemoryManager
{
    private const int MaxMessages = 10;

    public List<Message> BuildPromptMemory(
        IReadOnlyList<Message> history,
        Message newUserMessage)
    {
        var trimmedHistory = history
            .TakeLast(MaxMessages)
            .ToList();

        trimmedHistory.Add(newUserMessage);
        return trimmedHistory;
    }
}