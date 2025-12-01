using System.Net.Http.Headers;
using System.Text;

public class ElevenLabsService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public ElevenLabsService(IConfiguration config)
    {
        _apiKey = config["ElevenLabs:ApiKey"];
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("xi-api-key", _apiKey);
    }

    public async Task<byte[]> ConvertTextToSpeech(string text)
    {
        var voiceId = "21m00Tcm4TlvDq8ikWAM"; // Rachel (Multilingual)

        var url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";

        var json = new
        {
            text = text,
            model_id = "eleven_multilingual_v2",
            voice_settings = new
            {
                stability = 0.4,
                similarity_boost = 0.8
            }
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(json),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _http.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}
