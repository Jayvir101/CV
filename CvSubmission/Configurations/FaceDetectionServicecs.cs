using CvSubmission.Configurations;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class FaceDetectionService
{
    private readonly IFaceClient _faceClient;
    private readonly string _peopleFolderPath;

    public FaceDetectionService(IConfiguration configuration)
    {
        GlobalSettings.Bind(configuration);

        _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(GlobalSettings.AzureFaceRecognitionService.API_KEY))
        {
            Endpoint = GlobalSettings.AzureFaceRecognitionService.API_URL
        };

        // Set the path to the People folder
        _peopleFolderPath = @"D:\02. KHB\02.Internship(L&T)\00_Projects\Face Recognition\Files\People\People";
    }

    public async Task<(int, List<string>)> CountPeopleInGroupPhotoAsync(string imagePath)
    {
        try
        {
            using (Stream imageStream = File.OpenRead(imagePath))
            {
                IList<DetectedFace> faces = await _faceClient.Face.DetectWithStreamAsync(imageStream);
                int faceCount = faces.Count;

                List<string> identifiedPersons = new List<string>();

                // Identify persons if any faces detected
                if (faceCount > 0)
                {
                    List<Guid> faceIds = faces.Select(face => face.FaceId.Value).ToList();
                    IList<IdentifyResult> identifyResults = await _faceClient.Face.IdentifyAsync(faceIds, GlobalSettings.PersonGroupId);

                    foreach (var identifyResult in identifyResults)
                    {
                        foreach (var candidate in identifyResult.Candidates)
                        {
                            // Get the person name from the file name
                            string personId = candidate.PersonId.ToString();
                            string personName = GetPersonNameFromFileName(personId);

                            identifiedPersons.Add(personName);
                        }
                    }
                }

                return (faceCount, identifiedPersons);
            }
        }
        catch (APIErrorException ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            return (-1, new List<string>()); // or throw exception
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return (-1, new List<string>()); // or throw exception
        }
    }

    private string GetPersonNameFromFileName(string personId)
    {
        // Example logic to extract person name from file name
        string fileName = $"{personId}.jpg"; // Example file name format
        string filePath = Path.Combine(_peopleFolderPath, fileName);

        if (File.Exists(filePath))
        {
            return Path.GetFileNameWithoutExtension(filePath); // Return the name without extension
        }
        else
        {
            Console.WriteLine($"File {filePath} does not exist.");
            return string.Empty; // Return empty string if file not found
        }
    }
}
