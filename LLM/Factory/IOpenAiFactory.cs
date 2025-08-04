using Virtual_Assistant.LLM.Services.Interfaces;

namespace Virtual_Assistant.LLM.Factory
{
    public interface IOpenAiFactory
    {
        IOpenAIService GetService(OpenAiModel model);
    }

}
