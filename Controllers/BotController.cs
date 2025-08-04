using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Virtual_Assistant.LLM.Services.Interfaces;


namespace Virtual_Assistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class BotController : ControllerBase
    {
        //public IOpenAIService _openAIService;

        private readonly IBotService _botService;
        public BotController( IBotService botService)
        {
           
            _botService = botService;
        }


        [HttpGet("query")]
        public async Task<IActionResult> Prompt(String request)
        {
            var response= await _botService.QueryBot(request);
            var result = new
            {
                query = request,
                response = response
            };
            return Ok(result);
        }

    }
}
