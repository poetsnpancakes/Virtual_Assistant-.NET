using Virtual_Assistant.Database;
using Virtual_Assistant.Entity;
using System;
using Virtual_Assistant.LLM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;



namespace Virtual_Assistant.LLM.Services.Absolute
{
    public class ChatMemoryService : IChatMemoryService
    {
        private readonly ApplicationDbContext _context;

        public ChatMemoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(string sessionId, string role, string content)
        {
            var message = new ChatMessageEntity
            {
                SessionId = sessionId,
                Role = role,
                Content = content
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string sessionId)
        {
            var chatHistory= await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();


            var messages = new List<ChatMessage>();

    foreach (var message in chatHistory)
    {
        if (message.Role == "user")
            messages.Add(new UserChatMessage(message.Content));
                
        else if (message.Role == "assistant")
            messages.Add(new AssistantChatMessage( message.Content));
        else if (message.Role == "system")
            messages.Add(new SystemChatMessage(message.Content));
    }

    return messages;
        }

        public async Task ClearMessagesAsync(string sessionId)
        {
            var messages = _context.ChatMessages.Where(m => m.SessionId == sessionId);
            _context.ChatMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }

}
