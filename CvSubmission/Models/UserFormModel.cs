using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CvSubmission.Models
{
    public class UserForm
    {
        public string Name { get; set; }
        public IFormFile CV { get; set; }
    }

}
