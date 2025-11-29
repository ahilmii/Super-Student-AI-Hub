using System.Threading.Tasks;

namespace SuperStudentAIHub.Services
{
    public interface IImageService
    {
        // Changed from string to Task<string> for async API calls
        Task<string> GenerateImageUrl(string prompt);
    }
}