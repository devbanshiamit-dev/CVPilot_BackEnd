using CVPilotAPI.ResumeAnalyze;
using CVPilotAPI.ResumeService;
using Microsoft.AspNetCore.Mvc;

namespace CVPilotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeServices _resumeServices;

        public ResumeController(IResumeServices resumeServices)
        {
            _resumeServices = resumeServices;
        }
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeResume(IFormFile file)
        {
            var analysisResult = await _resumeServices.AnalysisFileAsync(file);
            return Ok(analysisResult);
        }
    }
}
