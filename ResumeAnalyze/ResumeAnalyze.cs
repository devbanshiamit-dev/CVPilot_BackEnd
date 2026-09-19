using CVPilotAPI.DTO;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace CVPilotAPI.ResumeAnalyze
{
    public class ResumeAnalyze : IResumeAnalyze
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration configuration;

        public ResumeAnalyze(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            this.configuration = configuration;
        }

        public async Task<ResumeAnalysisResponse> AnalyzeResumeAsync(string extractedText)
        {
            var apiModel = configuration["OpenAI:ApiModel"];
            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiModel))
                throw new InvalidOperationException("OpenAI API model is missing.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is missing.");

            if (string.IsNullOrWhiteSpace(extractedText))
                throw new ArgumentException("Resume text cannot be empty.", nameof(extractedText));

            const string url = "https://api.openai.com/v1/responses";

            var requestBody = new
            {
                model = apiModel,
                input = new object[]
                {
            new
            {
                role = "system",
                content = """
                    You are a professional resume analysis assistant.
                    Analyze the provided resume text and return ONLY a valid JSON object with this exact structure:
                    {
                      "score": int,
                      "profession": string,
                      "experience": string,
                      "skills": [string],
                      "problems": [string],
                      "suggestions": [string]
                    }
                    Do not include any extra text, markdown, or explanation.
                    """
            },
            new
            {
                role = "user",
                content = extractedText
            }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"OpenAI API failed with status {(int)response.StatusCode}: {responseContent}");
            }

            // Extract the actual JSON text from OpenAI response
            using var document = JsonDocument.Parse(responseContent);

            if (!document.RootElement.TryGetProperty("output", out var output) ||
                output.ValueKind != JsonValueKind.Array ||
                output.GetArrayLength() < 2)
            {
                throw new InvalidOperationException("Unexpected response structure from OpenAI (missing output).");
            }

            var message = output[1];

            if (!message.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.Array ||
                content.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Unexpected response structure from OpenAI (missing content).");
            }

            if (!content[0].TryGetProperty("text", out var textElement))
            {
                throw new InvalidOperationException("Unexpected response structure from OpenAI (missing text).");
            }

            var jsonText = textElement.GetString();

            if (string.IsNullOrWhiteSpace(jsonText))
                throw new InvalidOperationException("OpenAI returned empty analysis text.");

            // Deserialize the actual analysis JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ResumeAnalysisResponse>(jsonText, options);

            if (result is null)
                throw new InvalidOperationException("Failed to deserialize resume analysis response.");

            return result;
        }
    }
}
