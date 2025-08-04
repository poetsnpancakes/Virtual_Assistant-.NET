using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Virtual_Assistant.Models.Response;
using Virtual_Assistant.LLM.Services.Interfaces;
using System.Text.Json.Nodes;

namespace Virtual_Assistant.LLM.Twilio
{
    public static class StreamHandler
    {
        public static async Task Handle(WebSocket socket, IServiceProvider services)
        {
            var buffer = new byte[4096];
            var inputBuffer = new List<string>();
            Task? llmTask = null;
            CancellationTokenSource? cts = null;

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

                    var node = JsonNode.Parse(json);
                    List<VoiceMessage> messages = [];

                    if (node is JsonArray)
                        messages = JsonSerializer.Deserialize<List<VoiceMessage>>(json);
                    else if (node is JsonObject)
                    {
                        var single = JsonSerializer.Deserialize<VoiceMessage>(json);
                        if (single != null)
                            messages.Add(single);
                    }

                    if (messages.Count == 0)
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

                            // Cancel previous task if still running
                            if (cts != null && !cts.IsCancellationRequested)
                            {
                                cts.Cancel();
                                Console.WriteLine("🔴 Previous task cancelled.");
                            }

                            cts = new CancellationTokenSource();
                            var localCts = cts.Token;

                            llmTask = HandleAIResponse(socket, services, message, localCts);
                            _ = llmTask; // Optional: Await if you want to block
                        }
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("❌ JSON deserialization failed: " + ex.Message);
                    Console.WriteLine("Raw JSON: " + json);
                }
            }

            // Ensure graceful shutdown
            cts?.Cancel();
            if (llmTask != null)
                await llmTask;
        }

        private static async Task HandleAIResponse(WebSocket socket, IServiceProvider services, string prompt, CancellationToken token)
        {
            using var scope = services.CreateScope();
            var aiService = scope.ServiceProvider.GetRequiredService<IBotService>();

            try
            {
                var response = await aiService.QueryBot(prompt);
                Console.WriteLine($"🤖 AI Response: {response.Answer}");

                var payload = JsonSerializer.Serialize(new
                {
                    type = "text",
                    token = response.Answer,
                    last = false,
                    interruptible = true,
                });

                await socket.SendAsync(Encoding.UTF8.GetBytes(payload), WebSocketMessageType.Text, true, token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("🟡 AI response task was cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error while handling AI response: " + ex.Message);
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
