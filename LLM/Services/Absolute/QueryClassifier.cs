using Virtual_Assistant.LLM.Services.Interfaces;

namespace Virtual_Assistant.LLM.Services.Absolute
{
    public class QueryClassifier : IQueryClassifier
    {
        private readonly IOpenAIService _openAiService;

        public QueryClassifier(IOpenAIService openAiService)
        {
            _openAiService = openAiService;
        }

        
        public async Task<string> ClassifyQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }
            var prompt = $"""
        You are an intelligent classifier that decides whether a user query is:

        1. **semantic**: A factual, information-seeking, or knowledge-based question — e.g., definitions, lists, "what is", "who are", etc. These are often short, direct, or phrased as clear questions — even if just a single keyword (e.g., "Directors", "Price", "Features?","Resume").

        2. **general**: A conversational, vague, or context-based message — such as greetings, small talk, thanks, or messages relying on prior context (e.g., "Tell me more","What is your role","What about that?", "Thanks", "Hi").

        Examples:
        - "Hi, how are you?" → general  
        - "Thanks!" → general  
        - "Tell me more" → general  
        - "Directors" → semantic  
        - "Directors?" → semantic  
        - "What is the price?" → semantic  
        - "Can you help?" → general  
        - "Submit a resume" → semantic

        Query: "{query}"

        Return only one word: `semantic` or `general`.
        """;

            var response = await _openAiService.QueryAsync(prompt);
            return response;
        }


    }
}
