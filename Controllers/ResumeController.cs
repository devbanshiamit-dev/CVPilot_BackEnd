using CVPilotAPI.DTO;
using CVPilotAPI.ResumeAnalyze;
using CVPilotAPI.ResumeService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

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

        [HttpGet("analyze")]
        [EnableRateLimiting("FixedPolicy")]
        public async Task<IActionResult> AnalyzeResume(int resumeId)
        {
            var analysisResult = await _resumeServices.AnalysisFileAsync(resumeId);
            return Ok(analysisResult);
        }
        [HttpPost("upload")]
        [EnableRateLimiting("FixedPolicy")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            var resumeId = await _resumeServices.UploadResumeAsync(file);
            return Ok(new { ResumeId = resumeId });
        }
        [HttpGet("temp")]
        public IActionResult Temp()
        {
            var Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { Message = "Temp endpoint is working.", Claims = Claims });
        }
    }
}
