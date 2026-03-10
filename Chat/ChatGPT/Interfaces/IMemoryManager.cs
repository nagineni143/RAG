public interface IMemoryManager
{
    List<Message> BuildPromptMemory(
        IReadOnlyList<Message> history,
        Message newUserMessage);
}