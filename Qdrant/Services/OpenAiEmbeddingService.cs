using OpenAI;
using OpenAI.Embeddings;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;


namespace Qdrant.Services
{

   
    public class OpenAiEmbeddingService : IEmbeddingService
    {
        private readonly IConfiguration _config;

        public OpenAiEmbeddingService(IConfiguration config)
        {
            _config = config;

        }


        public async Task<float[]> GetEmbeddingAsync(string input)
        {
            var api = new OpenAIClient(_config["OPENAI_API_KEY"]);//all-MiniLM-L6-v2//text-embedding-3-small

            EmbeddingsResponse response = await api.EmbeddingsEndpoint.CreateEmbeddingAsync(input, "text-embedding-3-small");


            if (response == null || response.Data == null || response.Data.Count == 0)
            {
                throw new Exception("Failed to get embedding from OpenAI.");
            }

            // Return the embedding as a float array
            return response.Data[0].Embedding.Select(d => (float)d).ToArray();

        }

    }
}
