using CVPilotAPI.DTO;
using CVPilotAPI.Models;
using CVPilotAPI.ResumeAnalyze;

namespace CVPilotAPI.ResumeService
{
    public interface IResumeServices
    {
        Task<int> UploadResumeAsync(IFormFile file);
        Task<ResumeAnalysisResponse> AnalysisFileAsync(UserAnalysis analysis);
        Task<byte[]> DownloadResumeAsync(int resumeId);
    }
}
