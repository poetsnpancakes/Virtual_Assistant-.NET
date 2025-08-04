namespace Virtual_Assistant.LLM.Services.Interfaces
{
    public interface ILlm_rephrase
    {
        public Task<string> rephrase_query(string query);
    }
}
