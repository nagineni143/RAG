public class Message
{
    public string Role { get; init; }  // "system" | "user" | "assistant"
    public string Content { get; init; }
}