using CVPilotAPI.DTO;
using CVPilotAPI.ResumeAnalyze;

namespace CVPilotAPI.ResumeService
{
    public interface IResumeServices
    {
        Task<ResumeAnalysisResponse> AnalysisFileAsync(int ResumeId);
        Task<int> UploadResumeAsync(IFormFile file);
        Task<byte[]> DownloadResumeAsync(int Id);
    }
}
