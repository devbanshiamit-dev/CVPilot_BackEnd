using CVPilotAPI.ResumeAnalyze;

namespace CVPilotAPI.TextExtractionEngine
{
    public interface IResumeParserService
    {
        Task<string> ExtractTextFromFileAsync(string filePath);
    }
}
