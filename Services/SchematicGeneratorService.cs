using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using SuperStudentAIHub.Services;

public class SchematicGeneratorService : ISchematicService
{
    // C# Uyarı Giderimi: Null atanabilir yaptık.
    private readonly string? _apiKey;

    public SchematicGeneratorService(IConfiguration configuration)
    {
        _apiKey = configuration["Gemini:ApiKey"];

        // Hata Giderimi: Key yoksa durdur.
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("Gemini:ApiKey configuration not found or is empty.");
        }
    }

    // Arayüzden gelen GenerateMermaidCode metodunu uyguluyoruz.
    public async Task<string> GenerateMermaidCode(string inputText)
    {
        // API anahtarı kontrolü
        string apiKeyNotNull = _apiKey!;

        string finalPrompt =
            "Sen bir diyagram uzmanısın. Kullanıcının girdisini al ve bunun için yalnızca Mermaid.js formatında bir şema kodu üret. Kodun dışında hiçbir açıklama, başlık, Markdown kod bloğu veya ek metin ekleme. Kullanıcının isteği: " + inputText;

        using (var clientHttp = new HttpClient())
        {
            // DÜZELTME 1: Güncel model ismi "gemini-2.5-flash" olarak geri alındı.
            // Eğer bu da çalışmazsa "gemini-2.0-flash" deneyebilirsin.
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKeyNotNull}";

            // DÜZELTME 2: JSON yapısında "config" yerine "generationConfig" kullanıyoruz.
            // 400 hatasının asıl sebebi buydu.
            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new { text = finalPrompt }
                    }
                }
            },
                generationConfig = new
                {
                    temperature = 0.0
                }
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await clientHttp.PostAsync(url, content);

            // Hata ayıklama: Eğer yine hata alırsan, hatanın detayını görmek için bu bloğu kullanabilirsin.
            if (!response.IsSuccessStatusCode)
            {
                string errorDetail = await response.Content.ReadAsStringAsync();
                // Bu satır hatayı loglar veya fırlatır, böylece sorunun tam kaynağını (örn: yetki, kota) görebilirsin.
                throw new Exception($"API Hatası ({response.StatusCode}): {errorDetail}");
            }

            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<GeminiResponse>(responseString);

            string mermaidCode = jsonResponse?.candidates?[0]?.content?.parts?[0]?.text ?? string.Empty;

            mermaidCode = mermaidCode
                .Replace("```mermaid", "")
                .Replace("```", "")
                .Trim();

            return mermaidCode;
        }
    }
}

// REST API yanıtını ayrıştırmak için gereken basit sınıflar (Null güvenliği ile güncellendi)
public class GeminiResponse
{
    public Candidate[]? candidates { get; set; }
}

public class Candidate
{
    public Content? content { get; set; }
}

public class Content
{
    public Part[]? parts { get; set; }
}

public class Part
{
    public string? text { get; set; }
}