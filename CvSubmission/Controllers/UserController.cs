using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using CvSubmission.Helpers;
using CvSubmission.Models;
using System.IO;
using CvSubmission.Services;

public class UserController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly OpenAIService _openAIService;

    public UserController()
    {
        _openAIService = new OpenAIService(new HttpClient());
    }
   // public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
   // {
    //    _httpClientFactory = httpClientFactory;
   //     _configuration = configuration;
   // }

    [HttpGet]
    public IActionResult Cv()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Submit(UserForm model)
    {
        if (ModelState.IsValid)
        {
            string cvText;
            using (var memoryStream = new MemoryStream())
            {
                await model.CV.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                if (model.CV.FileName.EndsWith(".pdf"))
                {
                    cvText = PdfTextExtractor.ExtractText(memoryStream);
                }
                else if (model.CV.FileName.EndsWith(".docx"))
                {
                    cvText = DocxTextExtractor.ExtractText(memoryStream);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unsupported file format");
                    return View("Cv", model);
                }
            }

            var response = await _openAIService.AskQuestionAsync(model.Name, cvText);
            ViewBag.Response = response;
        }
        return View("Cv", model);
    }

   
}

