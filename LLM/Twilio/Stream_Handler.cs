using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Virtual_Assistant.Models.Response;
using Virtual_Assistant.LLM.Services.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Virtual_Assistant.LLM.Twilio
{
    public static class Stream_Handler
    {
        public static async Task Handle(WebSocket socket, IServiceProvider services)
        {
            var buffer = new byte[4096];
            var inputBuffer = new List<string>();
            Task? llmTask = null;
            CancellationTokenSource? cts = new CancellationTokenSource();

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"📨 Incoming JSON: [{json}]");



                try
                {
                    if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                    {
                        Console.WriteLine("⚠️ Empty or irrelevant JSON. Skipping.");
                        continue;
                    }

                    // Use JsonNode to detect whether the JSON is an array or object
                    var node = JsonNode.Parse(json);

                    List<VoiceMessage> messages = new();

                    if (node is JsonArray arr)
                    {
                        messages = JsonSerializer.Deserialize<List<VoiceMessage>>(json);
                        Console.WriteLine($"Deserialized JSON: [{messages}]");
                    }
                    else if (node is JsonObject obj)
                    {
                        var single = JsonSerializer.Deserialize<VoiceMessage>(json);
                        Console.WriteLine($"Deserialized JSON: [{single}]");
                        if (single != null)
                            messages.Add(single);
                    }

                    if (messages == null || messages.Count == 0)
                    {
                        Console.WriteLine("⚠️ No valid messages after deserialization.");
                        continue;
                    }

                    foreach (var data in messages)
                    {
                        Console.WriteLine($"✅ Deserialized: {data.Type} | {data.VoicePrompt}");

                        var message = CollectUtterance(data, inputBuffer);
                        if (!string.IsNullOrEmpty(message))
                        {
                            Console.WriteLine($"🎯 Final message: {message}");

                            cts = new CancellationTokenSource();

                            _ = Task.Run(async () =>
                            {
                                using var scope = services.CreateScope();
                                var aiService = scope.ServiceProvider.GetRequiredService<IBotService>();

                                var response = await aiService.QueryBot(message);
                                Console.WriteLine($"🤖 AI Response: {response.Answer}");

                                var payload = JsonSerializer.Serialize(new
                                {
                                    type = "text",
                                    token = response.Answer,
                                    last = false,
                                    interruptible = true,
                                });

                                await socket.SendAsync(Encoding.UTF8.GetBytes(payload), WebSocketMessageType.Text, true, CancellationToken.None);
                            }, cts.Token);
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("❌ JSON deserialization failed: " + ex.Message);
                    Console.WriteLine("Raw JSON: " + json);
                }


            }
        }

        private static string? CollectUtterance(VoiceMessage data, List<string> buffer)
        {
            if (data.Type == "prompt")
            {
                buffer.Add(data.VoicePrompt);
                if (data.Last)
                {
                    var utterance = string.Join(" ", buffer).Trim();
                    buffer.Clear();
                    return utterance;
                }
            }
            return null;
        }
    }
}

