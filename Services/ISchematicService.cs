using System.Threading.Tasks;

namespace SuperStudentAIHub.Services
{
    public interface ISchematicService
    {
        // Şema üretimi için kullanılan tek asenkron metot.
        Task<string> GenerateMermaidCode(string prompt);
    }
}