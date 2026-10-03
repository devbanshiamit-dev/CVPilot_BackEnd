using CVPilotAPI.DTO;
using CVPilotAPI.Models;
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
        private readonly IResumeService _resumeServices;

        public ResumeController(IResumeService resumeServices)
        {
            _resumeServices = resumeServices;
        }

        [HttpGet("analyze")]
        [EnableRateLimiting("FixedPolicy")]
        public async Task<IActionResult> AnalyzeResume(int resumeId)
        {
            var UserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0");

            var analysisResult = 
                await _resumeServices.AnalysisFileAsync(UserId, resumeId);

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
