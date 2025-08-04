using OpenAI.Chat;

namespace Virtual_Assistants.Service
{
    public class OpenAIService : IOpenAIService
    {
        //private readonly OpenAIClient _client;

        private readonly IConfiguration _config;


        public OpenAIService(IConfiguration config)
        {
            //_client = new OpenAIClient(apiKey);
            _config = config;
        }

        public async Task<string> QueryAsync(string userInput)
        {
            var OPENAI_API_KEY = _config["OPENAI_API_KEY"];



            if (string.IsNullOrEmpty(OPENAI_API_KEY))
            {
                throw new InvalidOperationException("API key is not configured.");
            }


            // var api = new OpenAIClient("API_KEY");


            /*  var chatClient = new ChatClient(
                model: "gpt-4o-mini",
                apiKey: OPENAI_API_KEY
               );*/

            //var result = await chatClient.CreateChatCompletionAsync(chatRequest);

            ChatClient client = new(model: "gpt-4o-mini", apiKey: OPENAI_API_KEY);
            ChatCompletion completion = await client.CompleteChatAsync(userInput);
            //var message = result.Value.Choices[0].Message.Content;
            return completion.Content[0].Text;

            //return message;
        }
    }
}
