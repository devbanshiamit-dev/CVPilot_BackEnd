using CVPilotAPI.DTO;
using CVPilotAPI.ResumeAnalyze;

namespace CVPilotAPI.ResumeService
{
    public interface IResumeServices
    {
        Task<ResumeAnalysisResponse> AnalysisFileAsync(IFormFile file);
        Task<byte[]> DownloadResumeAsync(string fileName);
    }
}
