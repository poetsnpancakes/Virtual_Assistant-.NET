namespace Virtual_Assistant.LLM.Services.Interfaces
{
    public interface IQdrantService
    {
        public Task<string> SearchAndSummarizeAsync(string query);

        public IAsyncEnumerable<string> ResponsiveSearchAndSummarizeAsync(string query);
    }
}
