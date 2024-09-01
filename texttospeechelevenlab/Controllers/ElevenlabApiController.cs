using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace texttospeechelevenlab.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElevenlabApiController : ControllerBase
    {
        private readonly string ElevenLabsApiKey;
        private readonly string apiUrl = "https://api.elevenlabs.io/v1/text-to-speech/";

        private readonly ILogger<ElevenlabApiController> _logger;

        public ElevenlabApiController(ILogger<ElevenlabApiController> logger, IConfiguration configuration)
        {
            ElevenLabsApiKey = configuration["ElevenLabs:APIKey"];
            _logger = logger;
        }
        [HttpGet("textspeech")]
        public async Task<IActionResult> GetTextToSpeech(string text)
        {
            byte[] audioBytes = await TextToSpeech(text);

            if (audioBytes != null)
            {

                //  var base64Content = Convert.ToBase64String(audioBytes); if you want you can change in base64 and return string

                return File(audioBytes, "audio/webm", "output_audio.mp3");
            }
            else
            {
                return BadRequest("Failed to generate audio.");
            }

        }
        private async Task<byte[]> TextToSpeech(string text)
        {
            var voiceId = "MF3mGyEYCl7XYWbV9V6O"; // choose voice from voice lib api 
            try
            {
                var body = new
                {
                    text = text,
                    model_id = "eleven_monolingual_v1",
                    voice_settings = new
                    {
                        stability = 0,
                        similarity_boost = 0,
                        style = 0.5,
                        use_speaker_boost = true
                    }
                };

                var jsonBody = System.Text.Json.JsonSerializer.Serialize(body);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("xi-api-key", ElevenLabsApiKey);
                    client.DefaultRequestHeaders.Add("accept", "audio/mpeg");

                    var response = await client.PostAsync(apiUrl + voiceId, new StringContent(jsonBody, Encoding.UTF8, "application/json"));
                    var responseContent = await response.Content.ReadAsStringAsync();
                  
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        //Console.WriteLine("Error: " + response.StatusCode);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

    }
}
