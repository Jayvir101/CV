using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace CvSubmission.Models
{
    public class FormEntryViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile Cv { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile Photo { get; set; }
        public string Highlights { get; set; }


        // List of form entries to be displayed in the view
        public List<FormEntry> Entries { get; set; } = new List<FormEntry>();

        // Pagination properties
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class FormEntry
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CvFileName { get; set; }

        [Required]
        public string PhotoFileName { get; set; }

        public string Highlights { get; set; }
    }
    public class SuccessViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhotoFileName { get; set; }
    }

}

