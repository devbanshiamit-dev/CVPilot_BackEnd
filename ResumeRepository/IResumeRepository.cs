using CVPilotAPI.Models;

namespace CVPilotAPI.Repository
{
    public interface IResumeRepository
    {
        Task<int> CreateResumeAsync(Resumes resume);
        Task<Resumes?> GetResumeByIdAsync(int resumeId);
        Task<bool> UpdateAsync(Resumes resume);
        Task<bool> DeleteAsync(int resumeId);
    }
}
