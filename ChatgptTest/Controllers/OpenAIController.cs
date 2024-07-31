using ChatgptTest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ChatgptTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly OpenAIService _openAIService;

        public OpenAIController(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        [HttpPost("ask")]
        public ActionResult<string> AskQuestion([FromBody] AskQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.CvText))
            {
                return BadRequest("Name and CvText cannot be empty.");
            }

            string response = _openAIService.GetOpenAIResponse(request.Name, request.CvText);
            return Ok(response);
        }

        [HttpPost("getinfo")]
        public ActionResult<string> GetInfo([FromBody] GetInfoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CvText))
            {
                return BadRequest("CvText cannot be empty.");
            }

            string response = _openAIService.GetInfo(request.CvText);
            return Ok(response);
        }


        public class AskQuestionRequest
        {
            public string Name { get; set; }
            public string CvText { get; set; }
        }
        public class GetInfoRequest
        {
           
            public string CvText { get; set; }
        }

    }
}
