namespace AIServiceRouter.Types;

public class ChatRequest
{
    public List<ChatMessage> Messages { get; set; } = new();
}