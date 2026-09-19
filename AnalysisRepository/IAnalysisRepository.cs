using CVPilotAPI.Models;

namespace CVPilotAPI.AnalysisRepository
{
    public interface IAnalysisRepository
    {
        Task<int> CreateAnalysisAsync(int resumeId, Analysis analysis);
        Task<Analysis?> GetAnalysisByResumeIdAsync(int resumeId);
        Task CreateSkillsAsync(Skills skills);
    }
}
