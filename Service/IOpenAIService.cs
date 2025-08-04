namespace Virtual_Assistants.Service
{
    public interface IOpenAIService
    {
        public Task<string> QueryAsync(string userInput);
    }
}
