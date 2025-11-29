using Microsoft.AspNetCore.Mvc;
using SuperStudentAIHub.Services;
using System.Threading.Tasks;

namespace SuperStudentAIHub.Controllers
{
    public class VisualsController : Controller
    {
        private readonly IChatGptService _chatService;
        private readonly IImageService _imageService;

        // Inject both the Chat Service (for Schematics) and Image Service (for Visuals)
        public VisualsController(IChatGptService chatService, IImageService imageService)
        {
            _chatService = chatService;
            _imageService = imageService;
        }

        // GET: /Visuals
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Visuals/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(string inputText, string visualType)
        {
            // 1. Pass the user's selection back to the View
            ViewBag.InputText = inputText;
            ViewBag.SelectedVisualType = visualType; // <--- NEW: Remember the selection

            if (string.IsNullOrWhiteSpace(inputText))
            {
                ViewBag.Error = "Please enter text to generate a visual.";
                return View("Index");
            }

            if (visualType == "schematic")
            {
                // Generate Mermaid.js Code
                string mermaidCode = await _chatService.GenerateSchematicAsync(inputText);
                ViewBag.ResultType = "schematic";
                ViewBag.SchematicCode = mermaidCode;
            }
            else if (visualType == "image")
            {
                try
                {
                    // Generate Image
                    string imageUrl = await _imageService.GenerateImageUrl(inputText);
                    ViewBag.ResultType = "image";
                    ViewBag.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    // 2. Capture the actual error message
                    // This often happens if the API Key is missing or invalid
                    ViewBag.Error = $"Image generation failed: {ex.Message}";
                }
            }

            return View("Index");
        }
    }
}