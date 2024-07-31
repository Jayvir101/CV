using System.Threading.Tasks;

namespace CvSubmission.Services
{
    public interface IOpenAIService
    {
        Task<string> AskQuestionAsync(string name, string cvText);

        Task<string> GetInfo(string cvText);
    }
}