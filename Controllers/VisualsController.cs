using Microsoft.AspNetCore.Mvc;
using SuperStudentAIHub.Services;
using System.Threading.Tasks;
using System; 

namespace SuperStudentAIHub.Controllers
{
    public class VisualsController : Controller
    {
        // ALAN TANIMLARI
        private readonly ISchematicService _schematicService; 
        private readonly IImageService _imageService; 

        // Constructor
        public VisualsController(ISchematicService schematicService, IImageService imageService)
        {
            _schematicService = schematicService;
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
            // Veri aktarımı ve boş kontrolü aynı kalır
            ViewBag.InputText = inputText;
            ViewBag.SelectedVisualType = visualType; 

            if (string.IsNullOrWhiteSpace(inputText))
            {
                ViewBag.Error = "Please enter text to generate a visual.";
                return View("Index");
            }

            if (visualType == "schematic")
            {
                // METHOD ADI DÜZELTİLDİ: GenerateSchematicAsync yerine GenerateMermaidCode
                string mermaidCode = await _schematicService.GenerateMermaidCode(inputText); 
                
                ViewBag.ResultType = "schematic";
                ViewBag.SchematicCode = mermaidCode;
            }
            else if (visualType == "image")
            {
                // ARKADAŞINIZIN KISMI: DEĞİŞMEDİ
                try
                {
                    string imageUrl = await _imageService.GenerateImageUrl(inputText);
                    ViewBag.ResultType = "image";
                    ViewBag.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Image generation failed: {ex.Message}";
                }
            }

            return View("Index");
        }
    }
}