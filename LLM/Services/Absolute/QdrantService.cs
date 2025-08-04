using System.Text.Json;
using Virtual_Assistant.LLM; // for OpenAiModel or related enums
using System.Net.Http.Json;
using Qdrant.Services;
using Virtual_Assistant.LLM.Services.Interfaces;
using Qdrant.Client;
using Qdrant.Models.Response;

namespace Virtual_Assistant.LLM.Services.Absolute
{


    public class QdrantService : IQdrantService
    {
        private readonly QdrantClient _qdrantClient;
        private readonly IEmbeddingService _embeddingService;
        private readonly IOpenAIService _openAiService;
        private readonly string _modelName;
        //private readonly IConfiguration _config;

        private readonly string[] _collections = ["careers", "servicesoffereds", "directorsinfo"];

        public QdrantService(QdrantClient qdrantClient, IEmbeddingService embeddingService, IConfiguration config
                              , IOpenAIService openAiService)
        {
            //_config = config;
            _qdrantClient = qdrantClient;
            _embeddingService = embeddingService;
            _openAiService = openAiService;//new OpenAIService(config, modelName); 
            //_modelName = modelName ?? "gpt-4o-mini"; // Default to gpt-4o-mini if null
        }

        private static string NormalizeText(string input)
        {
            return new string(input
                .Where(c => !char.IsPunctuation(c))
                .ToArray())
                .Trim()
                .ToLowerInvariant(); // optional: lowercase for consistency
        }

        public async Task<string> SearchAndSummarizeAsync(string query)
        {

            var normalizedPrompt = NormalizeText(query);
            // Step 1: Embed the query
            float[] queryVector = await _embeddingService.GetEmbeddingAsync(normalizedPrompt);

            // Step 2: Search all collections
            List<QdrantResult> allResults = new();

            foreach (var collection in _collections)
            {
                try
                {
                    var results = await _qdrantClient.SearchAsync(collection, queryVector, 5);
                    foreach (var result in results)
                    {
                        result.Payload["__collection"] = collection;
                        allResults.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error querying collection '{collection}': {ex.Message}");
                }
            }

            if (!allResults.Any())
            {
                return "No matching results found. Please contact us at info@grootsoftwares.com.";
            }

            // Step 3: Sort top 15 results
            var topResults = allResults
                .OrderByDescending(r => r.Score)
                .Take(15)
                .ToList();

            // Step 4: Build prompt for final summarization
            var payloads = topResults.Select(r => r.Payload).ToList();
            var jsonResults = JsonSerializer.Serialize(payloads, new JsonSerializerOptions { WriteIndented = true });

            var prompt = $"""
        You are GrootBot, an AI assistant for GrootNet Software Solutions.

        Your job is to answer customer questions based only on the search results below.
        
        - For services, direct users to https://grootsoftwares.com/services.
        - For careers, direct to https://grootsoftwares.com/career.
        - For resume submissions, say to email hr@grootsoftwares.com.
        - For anything else, or if no relevant results, direct to info@grootsoftwares.com.

        Question: {query}

        Results:
        {jsonResults}

        Generate a very short, accurate, human-friendly answer.
        """;

            // Step 5: Ask OpenAI to summarize
            var finalAnswer = await _openAiService.QueryAsync(prompt);

            return finalAnswer;

            //await foreach (var piece in _openAiService.ResponsiveQueryAsync(prompt))
            //{
            //    yield return piece; // Already a string
            //}
        }




        public async IAsyncEnumerable<string> ResponsiveSearchAndSummarizeAsync(string query)
        {
            var normalizedPrompt = NormalizeText(query);

            // Step 1: Embed the query
            float[] queryVector = await _embeddingService.GetEmbeddingAsync(normalizedPrompt);

            // Step 2: Search all collections
            List<QdrantResult> allResults = new();

            foreach (var collection in _collections)
            {
                try
                {
                    var results = await _qdrantClient.SearchAsync(collection, queryVector, 5);
                    foreach (var result in results)
                    {
                        result.Payload["__collection"] = collection;
                        allResults.Add(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error querying collection '{collection}': {ex.Message}");
                }
            }

            if (!allResults.Any())
            {
                yield return "Null";
            }

            // Step 3: Sort top 15 results
            var topResults = allResults
                .OrderByDescending(r => r.Score)
                .Take(15)
                .ToList();

            // Step 4: Build prompt for final summarization
            var payloads = topResults.Select(r => r.Payload).ToList();
            var jsonResults = JsonSerializer.Serialize(payloads, new JsonSerializerOptions { WriteIndented = true });

            var prompt = $"""
        You are GrootBot, an AI-assistant for GrootNet Software Solutions which is a Software Development Company(this means you are not a general-purpose AI, you ony answer questions related to Groot Software Solutions).
        You are given a list of search results from a database. Your task is to generate a short, readable answer in natural language based on the result.
        In case user queries about services offered,ask user to visit our website at https://grootsoftwares.com/services after showing results.
        In case user wants to submit a resume ask user to email at hr@grootsoftwares.com".
        In case user queries about job openings, ask user to visit our website at https://grootsoftwares.com/career after showing results.
        In case of an error or no results or question that are not company-related ask user to reach out to us at info@grootsoftwares.com 
        Question: {query}

        Results:
        {jsonResults}

        Generate a very short, accurate, human-friendly answer.
        """;

            // Step 5: Ask OpenAI to summarize
           // var finalAnswer = await _openAiService.QueryAsync(prompt);

            //return finalAnswer;

            await foreach (var piece in _openAiService.ResponsiveQueryAsync(prompt))
            {
                yield return piece; // Already a string
            }
        }
    }

}
