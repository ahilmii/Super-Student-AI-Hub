using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SuperStudentAIHub.Models;
using SuperStudentAIHub.Services;
using UglyToad.PdfPig;
using Xceed.Words.NET;



namespace SuperStudentAIHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IChatGptService _chat;
        private string ExtractPdf(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream);
            var text = new StringBuilder();

            foreach (var page in pdf.GetPages())
                text.AppendLine(page.Text);

            return text.ToString();
        }

        private string ExtractWord(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var mem = new MemoryStream();
            stream.CopyTo(mem);

            var doc = Xceed.Words.NET.DocX.Load(mem);
            return doc.Text;
        }



        public HomeController(ILogger<HomeController> logger, IChatGptService chat)
        {
            _logger = logger;
            _chat = chat;
        }

        public IActionResult Index(string? summaryResult = null)
        {
            ViewBag.SummaryResult = summaryResult;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Summarize(string inputText, string level)
        {
            var result = await _chat.SummarizeTextAsync(inputText, level);
            return RedirectToAction("Index", new { summaryResult = result });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public async Task<IActionResult> SummarizeFile(IFormFile uploadedFile, string level)
        {
            if (uploadedFile == null || uploadedFile.Length == 0)
                return RedirectToAction("Index", new { summaryResult = "No file uploaded." });

            string extractedText = "";

            var extension = Path.GetExtension(uploadedFile.FileName).ToLower();

            if (extension == ".pdf")
            {
                extractedText = ExtractPdf(uploadedFile);
            }
            else if (extension == ".docx")
            {
                extractedText = ExtractWord(uploadedFile);
            }
            else
            {
                return RedirectToAction("Index", new { summaryResult = "Unsupported file type." });
            }

            var result = await _chat.SummarizeTextAsync(extractedText, level);
            return RedirectToAction("Index", new { summaryResult = result });
        }

    }
}
