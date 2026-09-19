using CVPilotAPI.Models;

namespace CVPilotAPI.SuggestionRepository
{
    public interface ISuggestionRepository
    {
        Task<int> CreateSuggestionAsync(int analysisId, Suggestions suggestion);
        Task<Suggestions> GetSuggestionByAnalysisIdAsync(int analysisId);
    }
}
