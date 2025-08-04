using OpenAI.Chat;


namespace Virtual_Assistant.LLM.Services.Interfaces
{
    public interface IOpenAIService
    {
        public Task<string> QueryAsync(string userInput);
        public Task<string> ContextQueryAsync(List<ChatMessage> messages);
        public IAsyncEnumerable<string> ResponsiveQueryAsync(string userInput);

        public IAsyncEnumerable<string> ResponsiveContextQueryAsync(List<ChatMessage> messages);
    }

}
