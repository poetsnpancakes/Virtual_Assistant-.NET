using Virtual_Assistant.LLM.Services.Interfaces;
using Virtual_Assistant.Models.Response;
using OpenAI.Chat;
using System.Text;

namespace Virtual_Assistant.LLM.Services.Absolute
{
    public class BotService : IBotService
    {
        private readonly IQdrantService _qdrantService;
        private readonly IQueryClassifier _queryClassifier;
        private readonly ILlm_rephrase _llmRephrase;
        private readonly IOpenAIService _openAiService;
        private readonly IChatMemoryService _chatMemoryService;

        public BotService(IQdrantService qdrantService, IQueryClassifier queryClassifier,
            ILlm_rephrase llmRephrase, IOpenAIService openAiService, IChatMemoryService chatMemoryService)
        {
            _qdrantService = qdrantService;
            _queryClassifier = queryClassifier;
            _llmRephrase = llmRephrase;
            _openAiService = openAiService;
            _chatMemoryService = chatMemoryService;
        }



        public async Task<BotResponse> QueryBot(string query)
        {
            var answer = string.Empty;
            var rephrased_query = string.Empty;



        var bot_template = $"""
        You are GrootBot, an AI-assistant for Groot Software Solutions which is a Software Development Company.
        You provide users with company-related informations like company's services, company's job openings and company's team ,etc.
        Give response for the following user's question.

        Question: "{query}"
        """;
            // Classify the query
            string route = await _queryClassifier.ClassifyQuery(query);

           

            if (route == "semantic")
            {
                // Rephrase the query using LLM
                 rephrased_query = await _llmRephrase.rephrase_query(query);
                // Search and summarize using Qdrant with the rephrased query
                 answer = await _qdrantService.SearchAndSummarizeAsync(rephrased_query);

            }
            else
            {
                // 🧠 1. Get message history from memory service
                //List<ChatMessage> chatHistory = await _chatMemoryService.GetMessagesAsync(sessionId);
                // 🧠 2. Add user query to chat history
                //chatHistory.Add(new UserChatMessage(query));
                // 🧠 3. Create a system message with the chat history
                //chatHistory.Insert(0, new SystemChatMessage("You are GrootBot, an AI-assistant for Groot Software Solutions which is a Software Development Company." +
                    //"You provide users with company-related informations like company's services, company's job openings and company's team ,etc.Give response for the following user's question."));
                answer = await _openAiService.QueryAsync(bot_template);
            }

            // Save assistant reply
            //await _chatMemoryService.AddMessageAsync(sessionId, "assistant", answer);

            return new BotResponse
            {
                Query = query,
                Route = route,
                RephrasedQuery = rephrased_query ?? "General query, no rephrasing needed.",
                Answer = answer,
            };
        }

        public async IAsyncEnumerable<string> ResponsiveQueryBot(string query, string sessionId)
        {
            var answer = string.Empty;
            var rephrased_query = string.Empty;
            var fullReplyBuilder = new StringBuilder();


            var bot_template = $"""
        You are GrootBot, an AI-assistant for Groot Software Solutions which is a Software Development Company.
        You provide users with company-related informations like company's services, company's job openings and company's team ,etc.
        Give response for the following user's question.

        Question: "{query}"
        """;
            // Classify the query
            string route = await _queryClassifier.ClassifyQuery(query);



            if (route == "semantic")
            {
                // Rephrase the query using LLM
                rephrased_query = await _llmRephrase.rephrase_query(query);
                // Search and summarize using Qdrant with the rephrased query
                //answer = await _qdrantService.SearchAndSummarizeAsync(rephrased_query);
                await foreach (var piece in _qdrantService.ResponsiveSearchAndSummarizeAsync(rephrased_query))
                {
                    yield return piece; // Already a string
                }


            }
            else
            {
                // 🧠 1. Get message history from memory service
                List<ChatMessage> chatHistory = await _chatMemoryService.GetMessagesAsync(sessionId);
                // 🧠 2. Add user query to chat history
                chatHistory.Add(new UserChatMessage(query));
                // 🧠 3. Create a system message with the chat history
                chatHistory.Insert(0, new SystemChatMessage("You are GrootBot, an AI-assistant for Groot Software Solutions which is a Software Development Company." +
                    "You provide users with company-related informations like company's services, company's job openings and company's team ,etc.Give response for the following user's question."));
                // answer = await _openAiService.ContextQueryAsync(chatHistory);

                // Collect final reply while streaming

                await foreach (var piece in _openAiService.ResponsiveContextQueryAsync(chatHistory))
                {
                    fullReplyBuilder.Append(piece);
                    yield return piece; // Already a string
                }
                await _chatMemoryService.AddMessageAsync(sessionId, "assistant", fullReplyBuilder.ToString());
            }

            // Save assistant reply
            //await _chatMemoryService.AddMessageAsync(sessionId, "assistant", fullReplyBuilder.ToString());

            //return new BotResponse
            //{
            //    Query = query,
            //    Route = route,
            //    RephrasedQuery = rephrased_query ?? "General query, no rephrasing needed.",
            //    Answer = answer,
            //};
        }


    }
}
