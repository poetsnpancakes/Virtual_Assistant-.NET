using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Virtual_Assistant.LLM.Twilio;
using Virtual_Assistant.Models.Request;

namespace Virtual_Assistant.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class CallController : ControllerBase
    {
        public static string CallSid = "";

        [HttpPost]
        public IActionResult Post([FromForm] TwilioCallRequest request)
        {
            CallSid = request.CallSid;
            Console.WriteLine($"📞 Incoming call from {request.From}");

            var response = new VoiceResponse();
            var connect = new Connect();
            var relay = new ConversationRelay(
                url: $"{NgrokService.PublicUrl.Replace("https", "wss")}/stream",
                welcomeGreeting: "You are connected to AI assistant. How can I help you?"
            );
            relay.Language(code: "en-US", ttsProvider: "ElevenLabs", voice: "21m00Tcm4TlvDq8ikWAM");
            connect.Append(relay);
            response.Append(connect);

            return Content(response.ToString(), "text/xml");
        }
    }


}
