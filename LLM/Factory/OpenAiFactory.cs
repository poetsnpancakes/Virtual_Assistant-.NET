using Virtual_Assistant.LLM.Services.Absolute;
using Virtual_Assistant.LLM.Services.Interfaces;
using Qdrant.Client;
using Qdrant.Services;

namespace Virtual_Assistant.LLM.Factory
{
    public class OpenAiFactory : IOpenAiFactory
    {
        private readonly IConfiguration _config;
        private readonly QdrantClient _qdrantClient;
        private readonly IEmbeddingService _embeddingService;
        private readonly IOpenAIService _openAiService;
        private readonly string _modelName;

        //public OpenAiFactory(QdrantClient qdrantClient, IEmbeddingService embeddingService
        //    , IOpenAIService openAiService, IConfiguration config)
        //{
        //    _config = config;
        //    _qdrantClient = qdrantClient;
        //    _embeddingService = embeddingService;
        //    _openAiService = openAiService;
        //}
        public OpenAiFactory(IConfiguration config,string modelName)
        {
            _config = config;
            _modelName = modelName; //?? "gpt-4o-mini"; // Default to gpt-4o-mini if null
        }

        public IOpenAIService GetService(OpenAiModel model)
        {
            //var apiKey = _config["OPENAI_API_KEY"];

            //if (string.IsNullOrWhiteSpace(apiKey))
            //    throw new InvalidOperationException("OpenAI API key not found.");

            string modelName = model switch
            {
                OpenAiModel.Gpt4o => "gpt-4o",
                OpenAiModel.Gpt41 => "gpt-4.1",
                OpenAiModel.Gpt4oMini => "gpt-4o-mini"
                //_ => "gpt-4o-mini" // default fallback
            };

            return new OpenAIService(_config,modelName);
            //return new QdrantService(_qdrantClient, _embeddingService, _config, modelName);
        }
    }


}
