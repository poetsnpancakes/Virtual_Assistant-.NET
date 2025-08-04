using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Data;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;
using Virtual_Assistant.LLM.Services.Interfaces;



namespace Virtual_Assistant.LLM.Services.Absolute
{


    public class OpenAIService : IOpenAIService
    {
        //private readonly OpenAIClient _client;


        //private readonly string _apiKey;

        private readonly string _modelName;
        private readonly IConfiguration _config;

        //public OpenAIService(IConfiguration config)
        //{
        //    _config = config;
        //}


        public OpenAIService(IConfiguration config, string modelName)
        {
            _config = config;
            _modelName = modelName; //== null ? "gpt-4o-mini" : modelName; // Default to gpt-4o-mini if null
        }



        //private string GetModelName(OpenAiModel? model)
        //{
        //    return model switch
        //    {
        //        OpenAiModel.Gpt4o => "gpt-4o",
        //        OpenAiModel.Gpt41 => "gpt-4.1",
        //        OpenAiModel.Gpt4oMini or null=> "gpt-4o-mini",// ✅ default
        //        _ => "gpt-4o-mini"
        //    };
        //}

        public async Task<string> QueryAsync(string userInput)
        {
            //var OPENAI_API_KEY =  _config["OPENAI_API_KEY"];



            //if (string.IsNullOrEmpty(OPENAI_API_KEY))
            //{
            //    throw new InvalidOperationException("API key is not configured.");
            //}

            // string modelName = GetModelName(model);

            // var api = new OpenAIClient("API_KEY");


            /*  var chatClient = new ChatClient(
                model: "gpt-4o-mini",
                apiKey: OPENAI_API_KEY
               );*/

            //var result = await chatClient.CreateChatCompletionAsync(chatRequest);

            ChatClient client = new(model: _modelName, apiKey: _config["OPENAI_API_KEY"]);
            ChatCompletion completion = await client.CompleteChatAsync(userInput);
            //var message = result.Value.Choices[0].Message.Content;
            return completion.Content[0].Text;

            //return message;
        }

        public async Task<string> ContextQueryAsync(List<ChatMessage> messages)
        {
            ChatClient client = new(model: _modelName, apiKey: _config["OPENAI_API_KEY"]);

            //var chatRequest = new ChatRequest(messages);

            ChatCompletion completion = await client.CompleteChatAsync(messages);
            return completion.Content[0].Text;
        }



        public async IAsyncEnumerable<string> ResponsiveQueryAsync(string userInput)
        {
            

            ChatClient client = new(model: _modelName, apiKey: _config["OPENAI_API_KEY"]);

            //var updates = client.CompleteChatStreamingAsync(userInput);

            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(userInput);

            await foreach (var update in completionUpdates)
            {
                if (update.ContentUpdate.Count > 0)
                {
                    yield return update.ContentUpdate[0].Text;
                }
            }
        }


        public async IAsyncEnumerable<string> ResponsiveContextQueryAsync(List<ChatMessage> messages)
        {
            

            ChatClient client = new(model: _modelName, apiKey: _config["OPENAI_API_KEY"]);

            //var updates = client.CompleteChatStreamingAsync(messages);

            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(messages);

            await foreach (var update in completionUpdates)
            {
                if (update.ContentUpdate.Count > 0)
                {
                    yield return update.ContentUpdate[0].Text;
                }
            }
        }
    }

}
