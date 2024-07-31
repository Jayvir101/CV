using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.AspNetCore.Http;
using CvSubmission.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CvSubmission.Controllers
{
    public class GroupController : Controller
    {
        private readonly string RECOGNITION_MODEL = GlobalSettings.AzureFaceRecognitionService.RecognitionModel;
        private readonly string DETECTION_MODEL = GlobalSettings.AzureFaceRecognitionService.DetectionModel;
        private readonly string API_KEY = GlobalSettings.AzureFaceRecognitionService.API_KEY;
        private readonly string API_URL = GlobalSettings.AzureFaceRecognitionService.API_URL;
        private readonly string PersonGroupId = GlobalSettings.PersonGroupId;

        private readonly IFaceClient faceClient;
        private List<string> identifiedNames;

        public GroupController()
        {
            faceClient = new FaceClient(new ApiKeyServiceClientCredentials(API_KEY))
            {
                Endpoint = API_URL
            };
            identifiedNames = new List<string>();
        }

        [HttpPost]
        public async Task<IActionResult> UploadGroupImage(IFormFile img)
        {
            identifiedNames.Clear(); // Clear any previous results

            IList<Person> persons = await faceClient.PersonGroupPerson.ListAsync(PersonGroupId);

            if (persons.Count == 0)
            {
                TempData["errorMessage"] = "The system does not contain any trained persons!";
                return RedirectToAction("Index", "Group");
            }

            using (Stream imageStream = img.OpenReadStream())
            {
                IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithStreamAsync(
                    image: imageStream,
                    recognitionModel: RECOGNITION_MODEL,
                    detectionModel: DETECTION_MODEL);

                if (detectedFaces == null || detectedFaces.Count == 0)
                {
                    TempData["errorMessage"] = "The uploaded image does not appear to contain any human faces.";
                    return RedirectToAction("Index", "Group");
                }

                int totalPeopleDetected = detectedFaces.Count;

                // Sort detectedFaces by left coordinate to ensure left-to-right order
                detectedFaces = detectedFaces.OrderBy(face => face.FaceRectangle.Left).ToList();

                List<Guid> faceIds = detectedFaces.Select(face => face.FaceId.Value).ToList();

                IList<IdentifyResult> identifyResults = await faceClient.Face.IdentifyAsync(faceIds, PersonGroupId);

                foreach (var detectedFace in detectedFaces)
                {
                    IList<IdentifyResult> results = identifyResults.Where(result => result.FaceId == detectedFace.FaceId).ToList();

                    foreach (var result in results)
                    {
                        foreach (var candidate in result.Candidates)
                        {
                            Person person = await faceClient.PersonGroupPerson.GetAsync(PersonGroupId, candidate.PersonId);
                            if (person != null)
                            {
                                identifiedNames.Add(person.Name);
                            }
                        }
                    }
                }

                if (identifiedNames.Count == 0)
                {
                    TempData["errorMessage"] = "No known persons were identified in the group photo.";
                }
                else
                {
                    string identifiedMessage = $"Identified Persons from Left to Right are: {string.Join(", ", identifiedNames)}";
                    TempData["StatusMessage"] = identifiedMessage;
                }

                TempData["TotalPeopleDetected"] = $"Total number of people detected in the photo: {totalPeopleDetected}";
            }

            return RedirectToAction("Index", "Group");
        }

        public IActionResult Index()
        {
            ViewBag.StatusMessage = TempData["StatusMessage"];
            ViewBag.ErrorMessage = TempData["errorMessage"];
            ViewBag.TotalPeopleDetected = TempData["TotalPeopleDetected"];
            return View();
        }
    }
}
