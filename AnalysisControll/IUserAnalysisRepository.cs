using CVPilotAPI.Models;

namespace CVPilotAPI.AnalysisControll
{
    public interface IUserAnalysisRepository
    {
        Task<int> CreateUserAnalysisAsync(UserAnalysis analysis);
        Task<UserAnalysis?> GetUserAnalysisByIdAsync(int id);
        Task UpdateUserAnalysisAsync(UserAnalysis analysis);
    }
}
