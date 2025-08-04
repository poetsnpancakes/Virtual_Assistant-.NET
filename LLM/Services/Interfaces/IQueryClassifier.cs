namespace Virtual_Assistant.LLM.Services.Interfaces
{
    public interface IQueryClassifier
    {
        public Task<string> ClassifyQuery(string query);
    }
}
