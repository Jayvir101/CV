using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CvSubmission.Models
{
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
