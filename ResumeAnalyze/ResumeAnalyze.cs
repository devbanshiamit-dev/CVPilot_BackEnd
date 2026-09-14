using CVPilotAPI.DTO;
using System.Text.Json;
using System.Net.Http.Headers;

namespace CVPilotAPI.ResumeAnalyze
{
    public class ResumeAnalyze : IResumeAnalyze
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public ResumeAnalyze(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<ResumeAnalysisResponse> AnalyzeResumeAsync(ResumeAnalysisRequest request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var apiModel = _configuration["OpenAI:Model"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("AI API key is not configured.");

            if (string.IsNullOrWhiteSpace(apiModel))
                throw new InvalidOperationException("AI model is not configured.");

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = apiModel,
                input = """
            Analyze this resume for an ATS system.

            Return:
            - ATS score from 0 to 100
            - profession
            - experience
            - skills
            - problems in the resume
            - suggestions for improvement

            Resume:
            """ + request.ExtractedText,

                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = "resume_analysis",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                score = new
                                {
                                    type = "integer"
                                },
                                profession = new
                                {
                                    type = "string"
                                },
                                experience = new
                                {
                                    type = "string"
                                },
                                skills = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "string"
                                    }
                                },
                                problems = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "string"
                                    }
                                },
                                suggestions = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "string"
                                    }
                                }
                            },
                            required = new[]
                {
                    "score",
                    "profession",
                    "experience",
                    "skills",
                    "problems",
                    "suggestions"
                },
                            additionalProperties = false
                        }
                    }
                }
            };

            httpRequest.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(httpRequest);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"AI request failed: {response.StatusCode}");
            }

            using var document = JsonDocument.Parse(responseBody);

            var jsonText = document.RootElement
                .GetProperty("output")[1]
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                throw new InvalidOperationException(
                    "AI returned an empty analysis.");
            }

            var result = JsonSerializer.Deserialize<ResumeAnalysisResponse>(
                jsonText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null)
            {
                throw new InvalidOperationException(
                    "AI response could not be converted to ResumeAnalysisResponse.");
            }

            return result;
        }
    }
}
