using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperStudentAIHub.Services
{
    public class ChatGptService : IChatGptService
    {
        private readonly string _apiKey;
        private readonly HttpClient _http;

        public ChatGptService(IConfiguration config)
        {
            _apiKey = config["OpenAI:ApiKey"];
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> SummarizeTextAsync(string text, string level)
        {
            var prompt =
                $"Summarize the following text in {level} length. Then generate a 10‑item study list. The summary and the list must be in English only.\n\nText:\n{text}";



            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var result = await response.Content.ReadAsStringAsync();

            dynamic jsonResult = JsonConvert.DeserializeObject(result);

            return jsonResult.choices[0].message.content.ToString();
        }

        // Add this method inside your ChatGptService class

        public async Task<string> GenerateSchematicAsync(string text)
        {
            // We explicitly ask for Mermaid.js syntax and tell it to avoid Markdown formatting
            var prompt =
                $"Analyze the following text and generate a Mermaid.js diagram (graph TD, sequenceDiagram, or mindmap) that best represents the structure or concepts. " +
                $"Return ONLY the raw mermaid code string. Do not use markdown code blocks (```mermaid). Do not add explanations.\n\nText:\n{text}";

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
            new { role = "user", content = prompt }
        }
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("[https://api.openai.com/v1/chat/completions](https://api.openai.com/v1/chat/completions)", content);
            var result = await response.Content.ReadAsStringAsync();

            dynamic jsonResult = JsonConvert.DeserializeObject(result);
            string output = jsonResult.choices[0].message.content.ToString();

            // Cleanup: sometimes AI adds backticks anyway, so we remove them just in case
            return output.Replace("```mermaid", "").Replace("```", "").Trim();
        }
    }
}
