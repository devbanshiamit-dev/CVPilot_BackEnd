using CVPilotAPI.Models;

namespace CVPilotAPI.SuggestionRepository
{
    public interface ISuggestionRepository
    {
        Task<int> CreateSuggestionAsync(int analysisId, Suggestions suggestion);
        Task<int> CreateProblemAsync(int analysisId, Problem problem);
        Task<Suggestions> GetSuggestionByAnalysisIdAsync(int analysisId);
    }
}
