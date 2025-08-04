using Virtual_Assistant.Entity;
using OpenAI.Chat;

namespace Virtual_Assistant.LLM.Services.Interfaces
{
    public interface IChatMemoryService
    {
        Task AddMessageAsync(string sessionId, string role, string content);
        Task<List<ChatMessage>> GetMessagesAsync(string sessionId);
        Task ClearMessagesAsync(string sessionId);
    }

}
