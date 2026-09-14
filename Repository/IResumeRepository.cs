using CVPilotAPI.Models;

namespace CVPilotAPI.Repository
{
    public interface IResumeRepository
    {
        Task<int> CreateAsync(Resume resume);
        Task<Resume?> GetByIdAsync(int resumeId);
        Task<IEnumerable<Resume>> GetAllAsync();
        Task<bool> UpdateAsync(Resume resume);
        Task<bool> DeleteAsync(int resumeId);
    }
}
