using CVPilotAPI.DTO;
using CVPilotAPI.Models;
using CVPilotAPI.ResumeAnalyze;

namespace CVPilotAPI.ResumeService
{
    public interface IResumeService
    {
        Task<int> UploadResumeAsync(IFormFile file);
        Task<ResumeAnalysisResponse> AnalysisFileAsync(int UserId, int resumeId);
        Task<byte[]> DownloadResumeAsync(int resumeId);
    }
}
