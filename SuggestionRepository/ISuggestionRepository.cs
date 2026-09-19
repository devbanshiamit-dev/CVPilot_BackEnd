using CVPilotAPI.Models;

namespace CVPilotAPI.SuggestionRepository
{
    public interface ISuggestionRepository
    {
        Task<int> CreateSuggestionAsync(int analysisId, Suggestion suggestion);
        Task<Suggestion> GetSuggestionByAnalysisIdAsync(int analysisId);
    }
}
