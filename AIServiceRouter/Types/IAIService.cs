namespace AIServiceRouter.Types;

public interface IAIService
{
    string Name { get; }
    Task<IAsyncEnumerable<string>> ChatAsync(List<ChatMessage> messages);
}