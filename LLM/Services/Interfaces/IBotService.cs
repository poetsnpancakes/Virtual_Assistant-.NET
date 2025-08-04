using Virtual_Assistant.Models.Response;

namespace Virtual_Assistant.LLM.Services.Interfaces
{
    public interface IBotService
    {
        public Task<BotResponse> QueryBot(string query);

        public IAsyncEnumerable<string> ResponsiveQueryBot(string query, string sessionId);
    }
}
