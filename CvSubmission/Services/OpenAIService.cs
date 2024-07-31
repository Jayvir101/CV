using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using CvSubmission.Models;

namespace CvSubmission.Services
{

  
    public class OpenAIService: IOpenAIService
    {
        private readonly HttpClient _httpClient;

        public OpenAIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> AskQuestionAsync(string name, string cvText)
        {
            var request = new AskQuestionRequest
            {
                Name = name,
                CvText = cvText
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:44374/api/openai/ask", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error calling API");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetInfo(string cvText)
        {
            var request = new GetInfoRequest
            {
                
                CvText = cvText
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:44374/api/openai/getinfo", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error calling API");
            }

            return await response.Content.ReadAsStringAsync();
        }
    }

}
