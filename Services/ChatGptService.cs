using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json; // Newtonsoft yerine System.Text.Json kullanıyoruz (Daha modern)
using System.Threading.Tasks;
using System;

namespace SuperStudentAIHub.Services
{
    // Sınıf ismini ve Interface'i değiştirmedik, böylece HomeController hata vermez.
    public class ChatGptService : IChatGptService
    {
        private readonly string? _apiKey;

        public ChatGptService(IConfiguration configuration)
        {
            // BURASI ÖNEMLİ: Artık OpenAI değil, çalışan Gemini anahtarını çekiyoruz
            _apiKey = configuration["Gemini:ApiKey"];

            // Anahtar boşsa veya null ise uyarı verebiliriz ama akışı bozmayalım
        }

        public async Task<string> SummarizeTextAsync(string text, string level)
        {
            // Gemini için Prompt Hazırlığı
            // (OpenAI prompt yapısını Gemini'ye uygun düz metne çevirdik)
            string prompt = $"Summarize the following text in {level} length. Then generate a 10-item study list. The summary and the list must be in English only.\n\nText:\n{text}";

            return await CallGeminiAsync(prompt);
        }

        public async Task<string> GenerateSchematicAsync(string text)
        {
            // Diyagram Prompt Hazırlığı
            string prompt = $"Analyze the following text and generate a Mermaid.js diagram (graph TD, sequenceDiagram, or mindmap). Return ONLY the raw mermaid code string. No markdown, no explanations.\n\nText:\n{text}";

            return await CallGeminiAsync(prompt);
        }

        // Ortak Gemini API Çağrı Metodu
        private async Task<string> CallGeminiAsync(string prompt)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string apiKeyNotNull = _apiKey ?? "";

                    // VisualsController'da kullandığın ve çalışan model/adres
                    // Not: 2.5-flash çalışıyorsa onu, yoksa 1.5-flash kullanabilirsin.
                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKeyNotNull}";

                    // Gemini JSON Yapısı
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = prompt } } }
                        },
                        generationConfig = new { temperature = 0.7 }
                    };

                    string jsonContent = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);

                    // Eğer API hata dönerse (Örn: Yetki yok, kota doldu vs.)
                    if (!response.IsSuccessStatusCode)
                    {
                        return $"AI Servisi Hata Verdi: {response.StatusCode}. Lütfen anahtarını kontrol et.";
                    }

                    string responseString = await response.Content.ReadAsStringAsync();

                    // Yanıtı Ayrıştırma (Parsing)
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        try
                        {
                            // JSON hiyerarşisi: candidates[0] -> content -> parts[0] -> text
                            string result = doc.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text")
                                .GetString() ?? "Boş yanıt döndü.";

                            // Mermaid.js için markdown temizliği (```mermaid ve ``` silinir)
                            return result.Replace("```mermaid", "").Replace("```", "").Trim();
                        }
                        catch
                        {
                            return "API yanıtı ayrıştırılamadı (JSON Hatası).";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // İnternet yoksa veya kod patlarsa
                return $"Bağlantı Hatası: {ex.Message}";
            }
        }
    }
}