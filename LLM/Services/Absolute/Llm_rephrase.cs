using Virtual_Assistant.LLM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Virtual_Assistant.LLM.Services.Absolute
{
    public class Llm_rephrase:ILlm_rephrase
    {
        private readonly IOpenAIService _openAiService;

        public Llm_rephrase(IOpenAIService openAiService)
        {
            _openAiService = openAiService;
        }

        //var jsonResults = JsonSerializer.Serialize(payloads, new JsonSerializerOptions { WriteIndented = true });
        public async Task<string> rephrase_query(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }
        var prompt = $"""
        You are an intelligent query rephrasing model.
        Rephrase this user query to be more search-friendly for a vector database.
        This Vector database contains information about a Software Development Company, whose name is 'GrootNet Software Solutions'.
        In case there is query related to services, rephrase this query and search from 'servicesoffereds' collection.
        In case there is query related to careers or job openings, rephrase this query and search from 'careers' collection.
        In case there is query related to directors or founders or co-founders, rephrase this query and search from 'directorsinfo' collection.
        - Positions available in the company are listed under the 'CareerTitle' column in the 'Careers' collection.
        - Each job description is detailed in the 'ShortDescription' column of the 'Careers' collection.

        Query:'{query}'`
        """;

            var response = await _openAiService.QueryAsync(prompt);
            return response;
        }
    }
}
