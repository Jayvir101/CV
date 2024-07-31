using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatgptTest.Configurations
{
    public class AzureOpenAIServiceSettings
    {
        public string API_URL { get; set; }
        public string API_KEY { get; set; }
        public double TEMPERATURE { get; set; }
        public int MAX_TOKENS { get; set; }
        public double SAMPLING_FACTOR { get; set; }
        public int FREQUENCY_PENALTY { get; set; }
        public int PRESENCE_PENALTY { get; set; }
        public int SAMPLE_COUNT { get; set; }
    }

}
