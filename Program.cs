using Virtual_Assistant.Database;
using Microsoft.EntityFrameworkCore;
using Qdrant.Client;
using Virtual_Assistant.LLM.Factory;
using Virtual_Assistant.LLM.Services.Interfaces;
using Qdrant.Services;
using Virtual_Assistant.LLM.Services.Absolute;
using Virtual_Assistant.LLM.Twilio;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//Add Context with connection-string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("name_of_connection_string")));

builder.Services.AddHttpClient<QdrantClient>();

builder.Services.AddScoped<IOpenAiFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new OpenAiFactory(config, "gpt-4o-mini"); // or pick model dynamically if needed
});

builder.Services.AddScoped<IOpenAIService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new OpenAIService(config, "gpt-4o-mini");
});

builder.Services.AddScoped<IQueryClassifier, QueryClassifier>();
builder.Services.AddScoped<ILlm_rephrase, Llm_rephrase>();

builder.Services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
builder.Services.AddScoped<SessionIdProvider>();


builder.Services.AddScoped<IBotService, BotService>();

builder.Services.AddScoped<IQdrantService, QdrantService>();
builder.Services.AddScoped<IChatMemoryService, ChatMemoryService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<IOpenAIService, OpenAIService>();

var app = builder.Build();

int port = 5000;

// 1. Start ngrok
NgrokService.StartNgrok(port);

// 2. Configure Twilio Voice URL
string accountSid = builder.Configuration["TWILIO_ACCOUNT_SID"]!;
string authToken = builder.Configuration["TWILIO_AUTH_TOKEN"]!;
//string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN")!;
TwilioClient.Init(accountSid, authToken);

var phoneNumber = Twilio.Rest.Api.V2010.Account.IncomingPhoneNumberResource.Read().FirstOrDefault();
if (phoneNumber != null)
{
    Twilio.Rest.Api.V2010.Account.IncomingPhoneNumberResource.Update(
        pathSid: phoneNumber.Sid,
        voiceUrl: new Uri($"{NgrokService.PublicUrl}/call")
    );
    Console.WriteLine($"?? Waiting for calls on {phoneNumber.PhoneNumber}");
}

if (string.IsNullOrEmpty(NgrokService.PublicUrl))
{
    throw new Exception("Ngrok URL is empty.");
}
Console.WriteLine("Ngrok Public URL is: " + NgrokService.PublicUrl);

// now this won't throw
var voiceUri = new Uri($"{NgrokService.PublicUrl}/call");


// 3. Middleware
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/stream")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await StreamHandler.Handle(socket, context.RequestServices); // ?? inject scoped services
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});




//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();
app.Lifetime.ApplicationStopping.Register(() =>
{
    NgrokService.StopNgrok();
});


app.Run($"http://0.0.0.0:{port}");
