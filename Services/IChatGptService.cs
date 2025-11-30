using System.Threading.Tasks;

namespace SuperStudentAIHub.Services
{
    public interface IChatGptService
    {
        Task<string> SummarizeTextAsync(string text, string level);
        Task<string> GenerateSchematicAsync(string text);
    }
}
