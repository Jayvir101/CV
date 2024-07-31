using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AzureFaceRecognitionSettings
{
    public string API_URL { get; set; }
    public string API_KEY { get; set; }
    public string RecognitionModel { get; set; }
    public string DetectionModel { get; set; }

    public string PersonGroupId { get; set; }
}
