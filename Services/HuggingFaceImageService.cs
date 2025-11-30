using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace SuperStudentAIHub.Services
{
    public class HuggingFaceImageService : IImageService
    {
        private readonly string _apiKey;
        private readonly HttpClient _http;
        
        // We use the SDXL 1.0 model which is powerful and free on the Inference API
        private const string ModelUrl = "https://router.huggingface.co/hf-inference/models/stabilityai/stable-diffusion-xl-base-1.0";

        public HuggingFaceImageService(IConfiguration config)
        {
            // We will add this key to appsettings.json in the next step
            _apiKey = config["HuggingFace:ApiKey"]; 
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GenerateImageUrl(string prompt)
        {
            var payload = new
            {
                inputs = prompt
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(ModelUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback or error handling
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Hugging Face API Error: {error}");
            }

            // The API returns the raw image bytes (binary)
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            
            // Convert to Base64 so we can display it directly in the <img> tag
            var base64String = Convert.ToBase64String(imageBytes);
            
            // Return the data URI
            return $"data:image/jpeg;base64,{base64String}";
        }
    }
}