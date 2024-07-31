using System;
using CheckIfUserExists.Models;
using Microsoft.AspNetCore.Mvc;                 //name if user exist 0 if user does not exist
using Microsoft.Azure.CognitiveServices.Vision.Face;// old file is stored on clip named olduserfile
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace CheckIfUserExists.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IFaceClient _faceClient;
        private readonly AzureFaceRecognitionSettings _settings;
        private readonly ILogger<UserController> _logger;

        public UserController(IFaceClient faceClient, IOptions<AzureFaceRecognitionSettings> settings, ILogger<UserController> logger)
        {
            _faceClient = faceClient;
            _settings = settings.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }


        [HttpPost("CheckIfUserExists")]
        public async Task<IActionResult> CheckIfUserExists([FromBody] PhotoPathRequest request)
        {

            if (request == null || string.IsNullOrEmpty(request.PhotoPath))
            {
                return BadRequest("Please provide a valid photo path.");
            }
            if (!System.IO.File.Exists(request.PhotoPath))
            {
                return BadRequest("The provided photo path does not exist.");
            }
            try
            {
                bool result = true;
                using (var stream = System.IO.File.OpenRead(request.PhotoPath))
                {
                    IList<DetectedFace> detectedFaces = await _faceClient.Face.DetectWithStreamAsync(
                        image: stream,
                        recognitionModel: _settings.RecognitionModel,
                        detectionModel: _settings.DetectionModel);

                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        return Ok("2");
                    }

                    var faceId = detectedFaces[0]?.FaceId;
                    if (faceId != null && detectedFaces.Count==1)
                    {
                        string personGroupId = _settings.PersonGroupId;
                        IList<Guid> faceIds = new List<Guid> { faceId.Value };
                        IList<IdentifyResult> results = await _faceClient.Face.IdentifyAsync(faceIds, personGroupId);
                        _logger.LogInformation("Calling IdentifyAsync with faceIds: {FaceIds} and personGroupId: {PersonGroupId}", faceIds, personGroupId);

                        try
                        {
                            foreach (var identifyResult in results)
                            {
                                foreach (var candidate in identifyResult.Candidates)
                                {
                                    _logger.LogInformation("value of Personid: {}",candidate.PersonId);
                                    Person person = await _faceClient.PersonGroupPerson.GetAsync(personGroupId, candidate.PersonId);
                                    if (person != null)
                                    {

                                        result = true;
                                        var name = person.Name;
                                        return Ok($"User already exists by the name: {name}");
                                    }
                                }
                            }
                           
                        }
                        catch (APIErrorException apiEx)
                        {
                            _logger.LogError(apiEx, "Error calling Face API: {StatusCode} - {Message}", apiEx.Response.StatusCode, apiEx.Response.ReasonPhrase);
                            return StatusCode((int)apiEx.Response.StatusCode, $"Face API Error: {apiEx.Response.ReasonPhrase}");
                        }

                    }
                    else if (faceId != null && detectedFaces.Count > 1)
                    {
                        return Ok("3"); //For a group photo;
                    }
                }


                return Ok("0");

            }
            catch (APIErrorException apiEx)
            {
                _logger.LogError(apiEx, "Error calling Face API");
                return StatusCode(500, "Internal server error calling Face API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPost("AddUserToFaceRecognition")]
        public async Task<IActionResult> AddUserToFaceRecognition([FromBody] AddUserRequest request)
        {

            if (request == null || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.PhotoPath))
            {
                return BadRequest("Invalid request data.");
            }
            string personGroupId = _settings.PersonGroupId;
            IList<Person> persons = await _faceClient.PersonGroupPerson.ListAsync(personGroupId);
            if (persons == null || persons.Count == 0)
            {
                return BadRequest("The system does not contain any trained person.");
            }

            var newPerson = await _faceClient.PersonGroupPerson.CreateAsync(personGroupId, request.Name);
            using (Stream imageStream = new FileStream(request.PhotoPath, FileMode.Open, FileAccess.Read))
            {
                await _faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, newPerson.PersonId, imageStream, detectionModel: _settings.DetectionModel);
            }
            // Train the person group
            await _faceClient.PersonGroup.TrainAsync(personGroupId);
            TrainingStatus trainingStatus;
            do
            {
                trainingStatus = await _faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
                await Task.Delay(1000);
            } while (trainingStatus.Status == TrainingStatusType.Running);

            return Ok("1");
        }

        [HttpPost("DeletePersonByPhotoPath")]
        public async Task<IActionResult> DeletePersonByPhotoPath([FromBody] PhotoPathRequest request)
        {
            string personGroupId = _settings.PersonGroupId;
            if (request == null || string.IsNullOrEmpty(request.PhotoPath))
            {
                return BadRequest("Please provide a valid photo path.");
            }
            if (!System.IO.File.Exists(request.PhotoPath))
            {
                return BadRequest("The provided photo path does not exist.");
            }
            try
            {
               
                using (var stream = System.IO.File.OpenRead(request.PhotoPath))
                {
                    IList<DetectedFace> detectedFaces = await _faceClient.Face.DetectWithStreamAsync(
                        image: stream,
                        recognitionModel: _settings.RecognitionModel,
                        detectionModel: _settings.DetectionModel);

                    if (detectedFaces == null || detectedFaces.Count == 0)
                    {
                        BadRequest("No face.");
                    }

                    var faceId = detectedFaces[0]?.FaceId;
                    if (faceId == null)
                    {
                        return Ok("No valid face ID found.");
                    }
                    IList<Guid> faceIds = new List<Guid> { faceId.Value };
                    IList<IdentifyResult> results = await _faceClient.Face.IdentifyAsync(faceIds, personGroupId);
                    
                    foreach (var identifyResult in results)
                    {
                        foreach (var candidate in identifyResult.Candidates)
                        {
                            Guid personId = candidate.PersonId;

                            // Step 3: Delete all face IDs associated with the person
                            Person person = await _faceClient.PersonGroupPerson.GetAsync(personGroupId, personId);
                            foreach (var persistedFaceId in person.PersistedFaceIds)
                            {
                                await _faceClient.PersonGroupPerson.DeleteFaceAsync(personGroupId, personId, persistedFaceId);
                            }
                            _logger.LogInformation("Calling IdentifyAsync with Personid: {personId} and personGroupId: {PersonGroupId}", personId, personGroupId);
                            // Step 4: Delete the person
                            await _faceClient.PersonGroupPerson.DeleteAsync(personGroupId, personId);
                            _logger.LogInformation("Calling IdentifyAsync with Personid: {personId} and personGroupId: {PersonGroupId}", personId, personGroupId);


                            return Ok("1");//person deleted 
                        }
                    }
                }


                return Ok("0");

            }
            catch (APIErrorException apiEx)
            {
                _logger.LogError(apiEx, "Error calling Face API");
                return StatusCode(500, "Internal server error calling Face API");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, "Internal server error");
            }
        }



    }




    public class PhotoPathRequest
    {
        public string PhotoPath { get; set; }
    }
}





