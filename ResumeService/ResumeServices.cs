using CVPilotAPI.AnalysisControll;
using CVPilotAPI.AnalysisRepository;
using CVPilotAPI.DTO;
using CVPilotAPI.Models;
using CVPilotAPI.Repository;
using CVPilotAPI.ResumeAnalyze;
using CVPilotAPI.SuggestionRepository;
using CVPilotAPI.TextExtractionEngine;

namespace CVPilotAPI.ResumeService
{
    public class ResumeServices : IResumeServices
    {
        private readonly IResumeParserService _resumeParserService;
        private readonly IResumeAnalyze _resumeAnalysisService;
        private readonly IResumeRepository _resumeRepository;
        private readonly IAnalysisRepository _analysisRepository;
        private readonly ISuggestionRepository _suggestionRepository;
        private readonly IUserAnalysisRepository _userAnalysisRepository;

        public ResumeServices(
            IResumeParserService resumeParserService,
            IResumeAnalyze resumeAnalysisService,
            IResumeRepository resumeRepository,
            IAnalysisRepository analysisRepository,
            ISuggestionRepository suggestionRepository,
            IUserAnalysisRepository userAnalysisRepository)
        {
            _resumeParserService = resumeParserService;
            _resumeAnalysisService = resumeAnalysisService;
            _resumeRepository = resumeRepository;
            _analysisRepository = analysisRepository;
            _suggestionRepository = suggestionRepository;
            _userAnalysisRepository = userAnalysisRepository;
        }

        // ==================== Upload ====================

        public async Task<int> UploadResumeAsync(IFormFile file)
        {
            ValidateFileType(file.FileName);

            var filePath = await SaveUploadedFileAsync(file);
            var extractedText = await ExtractTextAsync(filePath);

            var resume = new Resumes
            {
                FileName = Path.GetFileName(filePath),
                FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
                FilePath = filePath,
                ExtractedText = extractedText
            };

            return await _resumeRepository.CreateResumeAsync(resume);
        }

        // ==================== Analysis ====================

        public async Task<ResumeAnalysisResponse> AnalysisFileAsync(UserAnalysis analysis)
        {
            await EnsureAnalysisAllowedAsync(analysis.UserId);

            var resume = await GetResumeForAnalysisAsync(analysis.ResumeId);

            var result = await _resumeAnalysisService
                .AnalyzeResumeAsync(resume.ExtractedText);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Failed to analyze the resume.");
            }

            await SaveAnalysisResultAsync(
                analysis.ResumeId,
                result);

            await IncrementAnalysisCountAsync(analysis.UserId);

            return result;
        }

        // ==================== Download ====================

        public async Task<byte[]> DownloadResumeAsync(int resumeId)
        {
            var resume = await _resumeRepository.GetResumeByIdAsync(resumeId);

            if (resume == null)
            {
                throw new ArgumentException(
                    $"Resume with ID {resumeId} not found.");
            }

            return await File.ReadAllBytesAsync(resume.FilePath);
        }

        // ==================== File Helpers ====================

        private void ValidateFileType(string fileName)
        {
            string[] allowedExtensions =
            {
                ".pdf",
                ".doc",
                ".docx",
                ".txt"
            };

            var extension = Path.GetExtension(fileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Invalid file type. Only TXT, PDF, DOC, and DOCX files are allowed.");
            }
        }

        private async Task<string> SaveUploadedFileAsync(IFormFile file)
        {
            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads");

            Directory.CreateDirectory(uploadFolder);

            var fileName =
                $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName);

            await using var stream =
                new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            return filePath;
        }

        private async Task<string> ExtractTextAsync(string filePath)
        {
            var extractedText =
                await _resumeParserService
                    .ExtractTextFromFileAsync(filePath);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                throw new InvalidOperationException(
                    "Failed to extract text from the resume.");
            }

            return extractedText;
        }

        // ==================== Resume Helpers ====================

        private async Task<Resumes> GetResumeForAnalysisAsync(int resumeId)
        {
            var resume =
                await _resumeRepository.GetResumeByIdAsync(resumeId);

            if (resume == null)
            {
                throw new ArgumentException(
                    $"Resume with ID {resumeId} not found.");
            }

            if (string.IsNullOrWhiteSpace(resume.ExtractedText))
            {
                throw new InvalidOperationException(
                    "Extracted text is null or empty.");
            }

            return resume;
        }

        // ==================== Analysis Save ====================

        private async Task SaveAnalysisResultAsync(
            int resumeId,
            ResumeAnalysisResponse result)
        {
            var analysisId =
                await _analysisRepository.CreateAnalysisAsync(
                    resumeId,
                    new Analysis
                    {
                        ResumeId = resumeId,
                        Score = result.Score,
                        Profession = result.Profession,
                        Experience = result.Experience
                    });

            await SaveSkillsAsync(resumeId, result.Skills);
            await SaveSuggestionsAsync(analysisId, result.Suggestions);
            await SaveProblemsAsync(analysisId, result.Problems);
        }

        private async Task SaveSkillsAsync(
            int resumeId,
            List<string> skills)
        {
            foreach (var skill in skills)
            {
                await _analysisRepository.CreateSkillsAsync(
                    new Skills
                    {
                        ResumeId = resumeId,
                        Skill = skill
                    });
            }
        }

        private async Task SaveSuggestionsAsync(
            int analysisId,
            List<string> suggestions)
        {
            foreach (var suggestion in suggestions)
            {
                await _suggestionRepository.CreateSuggestionAsync(
                    analysisId,
                    new Suggestions
                    {
                        AnalysisId = analysisId,
                        Suggestion = suggestion
                    });
            }
        }

        private async Task SaveProblemsAsync(
            int analysisId,
            List<string> problems)
        {
            foreach (var problem in problems)
            {
                await _suggestionRepository.CreateProblemAsync(
                    analysisId,
                    new Problem
                    {
                        AnalysisId = analysisId,
                        problem = problem
                    });
            }
        }

        // ==================== Analysis Limit ====================

        private async Task<UserAnalysis> GetUserAnalysisAsync(int userId)
        {
            var userAnalysis =
                await _userAnalysisRepository
                    .GetUserAnalysisByIdAsync(userId);

            if (userAnalysis != null)
            {
                return userAnalysis;
            }

            userAnalysis = new UserAnalysis
            {
                UserId = userId,
                AnalysisCount = 0,
                WindowStartedAt = DateTime.UtcNow
            };

            await _userAnalysisRepository
                .CreateUserAnalysisAsync(userAnalysis);

            return userAnalysis;
        }

        private async Task EnsureAnalysisAllowedAsync(int userId)
        {
            var userAnalysis =
                await GetUserAnalysisAsync(userId);

            var now = DateTime.UtcNow;

            if (now - userAnalysis.WindowStartedAt
                >= TimeSpan.FromHours(24))
            {
                userAnalysis.AnalysisCount = 0;
                userAnalysis.WindowStartedAt = now;

                await _userAnalysisRepository
                    .UpdateUserAnalysisAsync(userAnalysis);

                return;
            }

            if (userAnalysis.AnalysisCount >= 2)
            {
                throw new InvalidOperationException(
                    "You have reached the maximum number of analyses. Please try again after 24 hours.");
            }
        }

        private async Task IncrementAnalysisCountAsync(int userId)
        {
            var userAnalysis =
                await GetUserAnalysisAsync(userId);

            userAnalysis.AnalysisCount++;

            await _userAnalysisRepository
                .UpdateUserAnalysisAsync(userAnalysis);
        }
    }
}