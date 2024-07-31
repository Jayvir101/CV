using CvSubmission.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.IO; 
using System.Threading.Tasks; // Add this namespace for Task
using System.Collections.Generic; // Add this namespace for List
using System.Linq; // Add this namespace for LINQ
using System.Net.Http;//old home file
using System.Net.Http.Json;
using System;
using CvSubmission.Helpers;
using CvSubmission.Services;

namespace CvSubmission.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        private readonly IHttpClientFactory _httpClientFactory;
        private const int PageSize = 11;
        private readonly IOpenAIService _openAIService;
        public HomeController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IOpenAIService openAIService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClientFactory = httpClientFactory;
            _openAIService = openAIService;
        }

        public IActionResult FormEntry()
        {
            return View();
        }

        public IActionResult Entry()
        {
            return View("FormEntry");
        }

        public IActionResult Help()
        {
            return View("Help");
        }

        //public IActionResult FormEntries()
        //{
        //    var entries = GetFormEntriesFromDatabase();
        //    return View(entries);
        //}

        [HttpPost]
        public async Task<IActionResult> SubmitForm(FormEntryViewModel model)
        {
            if (ModelState.IsValid)
            {
                string highlightsValue;
                var photoFilePath = Path.Combine("wwwroot/uploads/photo", model.Photo.FileName);

                var absolutePath = Path.GetFullPath(photoFilePath);

               // var cvfilePath = Path.Combine("wwwroot/uploads/photo", model.Cv.FileName);
                //var AbsoluteCvPath = Path.GetFullPath(cvfilePath);

                using (var stream = new FileStream(photoFilePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }


                var userExists = await CheckIfUserExists(absolutePath);
                if (userExists == "0")
                {
                    string cvText;
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Cv.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        if (model.Cv.FileName.EndsWith(".pdf"))
                        {
                            cvText = PdfTextExtractor.ExtractText(memoryStream);
                        }
                    
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Unsupported Cv file format: Only .pdfs are supported");
                            return View("FormEntry", model);
                        }
                    }

                    var response = await _openAIService.AskQuestionAsync(model.Name, cvText);
                    string firstThreeLetters = response.Substring(0, 3);
                    string finalres = firstThreeLetters.ToLower();
                    //ViewBag.Response = response;

                    if (finalres == "yes")
                    {
                        highlightsValue = await _openAIService.GetInfo(cvText);
                        var userAdded = await AddUserToFaceRecognition(model.Name, absolutePath);
                        if (!userAdded)
                        {
                            ModelState.AddModelError(string.Empty, "Failed to add user to face recognition.");
                            return View("FormEntry", model);
                        }

                    }
                    else
                    {
                        ViewBag.Response = response;
                        ModelState.AddModelError(string.Empty, response);
                        return View("FormEntry", model);

                    }


                   
                }
                else if (userExists == "2")
                {
                    ModelState.AddModelError(string.Empty, "No Face detected in the Photo.");
                    return View("FormEntry", model);
                }
                else if (userExists == "3")
                {
                    ModelState.AddModelError(string.Empty, "Group Photo is not allowed. ");
                    return View("FormEntry", model);
                }
                else
                {
                    string errorMessage = userExists;
                    ModelState.AddModelError(string.Empty, errorMessage);
                    return View("FormEntry", model);
                }

                var cvFilePath = Path.Combine("wwwroot/uploads/cv", model.Cv.FileName);

                using (var stream = new FileStream(cvFilePath, FileMode.Create))
                {
                    await model.Cv.CopyToAsync(stream);
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO FormEntries (Name, Email, CvFileName, PhotoFileName, Highlights) VALUES (@Name, @Email, @CvFileName, @PhotoFileName, @Highlights)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", model.Name);
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@CvFileName", model.Cv.FileName);
                        cmd.Parameters.AddWithValue("@PhotoFileName", model.Photo.FileName);
                        cmd.Parameters.AddWithValue("@Highlights", highlightsValue);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["SuccessMessage"] = "Form has been submitted successfully";
                var successModel = new SuccessViewModel
                {
                    Name = model.Name,
                    Email = model.Email,
                    PhotoFileName = model.Photo.FileName
                };
                return RedirectToAction("Success",successModel);
            }

            return View("FormEntry", model);
        }
        public IActionResult Success(SuccessViewModel model)
        {
            return View(model);
        }



        private async Task<string> CheckIfUserExists(string photoPath)
        {
            var client = _httpClientFactory.CreateClient("UserApiClient");
            var request = new PhotoPathRequest { PhotoPath = photoPath };
            var response = await client.PostAsJsonAsync("api/User/CheckIfUserExists", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result; //int.Parse(result); // Assuming the API returns "0" or "1" as a string
            }
            else
            {
                // Log the error or handle it as needed
                var errorContent = await response.Content.ReadAsStringAsync();
                // Optionally log errorContent or response.StatusCode for debugging
                // _logger.LogError($"API call failed with status code {response.StatusCode}: {errorContent}");

                // Return -1 to indicate failure
                return errorContent;
            }
        }




        private async Task<bool> AddUserToFaceRecognition(string name, string photoPath)
        {
            var client = _httpClientFactory.CreateClient("UserApiClient");
            var request = new AddUserRequest { Name = name, PhotoPath = photoPath };
            var response = await client.PostAsJsonAsync("api/User/AddUserToFaceRecognition", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return result == "1";
            }

            return false;
        }


       // [HttpGet]
        //public async Task<IActionResult> FormEntries(int pageNumber = 1, string searchTerm = null)
        //{
        //    List<FormEntry> entries;
        //    int totalEntries;

        //    if (string.IsNullOrEmpty(searchTerm))
        //    {
        //        entries = GetFormEntriesFromDatabase(pageNumber, PageSize);
        //        totalEntries = GetTotalEntriesCount();
        //    }
        //    else
        //    {
        //        entries = SearchFormEntriesFromDatabase(searchTerm, pageNumber, PageSize);
        //        totalEntries = GetTotalEntriesCount(searchTerm);
        //    }

        //    var viewModel = new FormEntryViewModel
        //    {
        //        Entries = entries,
        //        CurrentPage = pageNumber,
        //        TotalPages = (int)Math.Ceiling(totalEntries / (double)PageSize)
        //    };

        //    ViewData["searchTerm"] = searchTerm;

        //    return View(viewModel);
        //}
        [HttpGet]
        public async Task<IActionResult> FormEntries(int pageNumber = 1, string searchTerm = null)
        {
            List<FormEntry> entries;
            int totalEntries;

            if (string.IsNullOrEmpty(searchTerm))
            {
                entries = GetFormEntriesFromDatabase(pageNumber, PageSize);
                totalEntries = GetTotalEntriesCount();
            }
            else
            {
                entries = SearchFormEntriesFromDatabase(searchTerm, pageNumber, PageSize);
                totalEntries = GetTotalEntriesCount(searchTerm);
            }

            var viewModel = new FormEntryViewModel
            {
                Entries = entries,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalEntries / (double)PageSize)
            };

            ViewData["searchTerm"] = searchTerm;

            return View(viewModel);
        }

        private List<FormEntry> GetFormEntriesFromDatabase(int pageNumber, int pageSize)
        {
            var entries = new List<FormEntry>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT Id, Name, Email, CvFileName, PhotoFileName, Highlights
        FROM FormEntries
        ORDER BY Name
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(new FormEntry
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                CvFileName = reader.GetString(3),
                                PhotoFileName = reader.GetString(4),
                                Highlights = reader.IsDBNull(5) ? null : reader.GetString(5) // Handle NULL values
                            });
                        }
                    }
                }
            }

            return entries;
        }

        //private List<FormEntry> GetFormEntriesFromDatabase(int pageNumber, int pageSize)
        //{
        //    var entries = new List<FormEntry>();

        //    using (SqlConnection conn = new SqlConnection(_connectionString))
        //    {
        //        string query = @"
        //        SELECT Id, Name, Email, CvFileName, PhotoFileName
        //        FROM FormEntries
        //        ORDER BY Name
        //        OFFSET @Offset ROWS
        //        FETCH NEXT @PageSize ROWS ONLY";

        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        //            cmd.Parameters.AddWithValue("@PageSize", pageSize);

        //            conn.Open();
        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    entries.Add(new FormEntry
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        Name = reader.GetString(1),
        //                        Email = reader.GetString(2),
        //                        CvFileName = reader.GetString(3),
        //                        PhotoFileName = reader.GetString(4)
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return entries;
        //}

        private List<FormEntry> SearchFormEntriesFromDatabase(string searchTerm, int pageNumber, int pageSize)
        {
            var entries = new List<FormEntry>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT Id, Name, Email, CvFileName, PhotoFileName
                FROM FormEntries
                WHERE Name LIKE @SearchTerm
                ORDER BY Name
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(new FormEntry
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                CvFileName = reader.GetString(3),
                                PhotoFileName = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return entries;
        }

        private int GetTotalEntriesCount(string searchTerm = null)
        {
            int count = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = string.IsNullOrEmpty(searchTerm)
                    ? "SELECT COUNT(*) FROM FormEntries"
                    : "SELECT COUNT(*) FROM FormEntries WHERE Name LIKE @SearchTerm";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    }

                    conn.Open();
                    count = (int)cmd.ExecuteScalar();
                }
            }

            return count;
        }

        //Code For deleting Entries from the database 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string photo="";
                

               
                connection.Open();

                string selectSql = "SELECT PhotoFileName FROM FormEntries WHERE Id = @Id";

                using (var selectCommand = new SqlCommand(selectSql, connection))
                {
                    selectCommand.Parameters.AddWithValue("@Id", id);

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            photo = reader["PhotoFileName"].ToString();
                        }
                    }
                }
                var PhotoFilePath = Path.Combine("wwwroot/uploads/photo", photo);
                var absolutePath = Path.GetFullPath(PhotoFilePath);
                var AzureDelete = await DeletePersonByPhotoPath(absolutePath);

                if (AzureDelete == "1")
                {
                    // Example delete query
                    string sql = "DELETE FROM FormEntries WHERE Id = @Id";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Error: Unable to delete data";
                }
            }

            return RedirectToAction("FormEntries");
        }


        public async Task<String> DeletePersonByPhotoPath(string photoPath)
        {
            var client = _httpClientFactory.CreateClient("UserApiClient");
            var request = new PhotoPathRequest { PhotoPath = photoPath };
            

            var response = await client.PostAsJsonAsync("api/User/DeletePersonByPhotoPath/", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                // Process success
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                return errorContent;
            }
        }
    }



    public class PhotoPathRequest
    {
        public string PhotoPath { get; set; }
    }

    public class AddUserRequest
    {
        public string Name { get; set; }
        public string PhotoPath { get; set; }
    }
}


























































