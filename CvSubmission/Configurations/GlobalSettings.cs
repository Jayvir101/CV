using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CvSubmission.Configurations
{
    public static class GlobalSettings
    {
        public static class AzureFaceRecognitionService
        {
            public static string API_URL { get; set; }
            public static string API_KEY { get; set; }
            public static string RecognitionModel { get; set; }
            public static string DetectionModel { get; set; }
        }

        public static string PersonGroupId { get; set; }

        public static void Bind(IConfiguration configuration)
        {
            var azureSection = configuration.GetSection("AzureFaceRecognitionService");
            AzureFaceRecognitionService.API_URL = azureSection["API_URL"];
            AzureFaceRecognitionService.API_KEY = azureSection["API_KEY"];
            AzureFaceRecognitionService.RecognitionModel = azureSection["RecognitionModel"];
            AzureFaceRecognitionService.DetectionModel = azureSection["DetectionModel"];
            PersonGroupId = configuration["PersonGroupId"];
        }
    }

}
