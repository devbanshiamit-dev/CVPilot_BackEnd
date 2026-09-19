using CVPilotAPI.DTO;

namespace CVPilotAPI.ResumeAnalyze
{
    public interface IResumeAnalyze
    {
        Task<ResumeAnalysisResponse> AnalyzeResumeAsync(string extractedText);
    }
}
