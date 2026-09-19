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
        public ResumeServices(
            IResumeParserService resumeParserService, 
            IResumeAnalyze resumeAnalysisService, 
            IResumeRepository resumeRepository,
            IAnalysisRepository analysisRepository,
            ISuggestionRepository suggestionRepository)
        {
            _resumeParserService = resumeParserService;
            _resumeAnalysisService = resumeAnalysisService;
            _resumeRepository = resumeRepository;
            _analysisRepository = analysisRepository;
            _suggestionRepository = suggestionRepository;
        }

        //File Download Methods
        public async Task<byte[]> DownloadResumeAsync(string fileName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);
            byte[] fileBytes = await File.ReadAllBytesAsync(path);
            return fileBytes;
        }

        //Resume Analysis Methods
        public async Task<ResumeAnalysisResponse> AnalysisFileAsync(int ResumeId)
        {
            var DbResume = await _resumeRepository.GetResumeByIdAsync(ResumeId);

            if (DbResume == null)
            {
                throw new ArgumentException($"Resume with ID {ResumeId} not found.");
            }

            if (string.IsNullOrEmpty(DbResume.ExtractedText))
            {
                throw new InvalidOperationException("Extracted text is null or empty.");
            }

            var result = await _resumeAnalysisService.AnalyzeResumeAsync(DbResume.ExtractedText);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to analyze the resume.");
            }

            await _analysisRepository.CreateAnalysisAsync(ResumeId, new Analysis
            {
                ResumeId = ResumeId,
                Score = result.Score,
                Profession = result.Profession,
                Experience = result.Experience,
                Skills = result.Skills,
            });

            await _suggestionRepository.CreateSuggestionAsync(ResumeId, new Suggestion
            {
                AnalysisId = ResumeId,
                Suggestions = result.Suggestions,
                Problems = result.Problems
            });

            return result;
        }
        //file Upload Methods
        public async Task<int> UploadResumeAsync(IFormFile file)
        {
            if (!IsValidFileType(file.FileName))
            {
                throw new ArgumentException(
                    "Invalid file type. Only TXT, PDF, DOC, and DOCX files are allowed."
                );
            }

            var filename = Guid.NewGuid().ToString() +
                           Path.GetExtension(file.FileName);

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads"
            );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var path = Path.Combine(uploadFolder, filename);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = await ExtractTextAsync(path);

            int resumeId = await _resumeRepository.CreateResumeAsync(new Resumes
            {
                FileName = filename,
                FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
                FilePath = path,
                ExtractedText = extractedText
            });

            return resumeId;
        }
        //Text Extraction Methods
        private async Task<String> ExtractTextAsync(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);

            if (!IsValidFileType(fileName))
            {
                throw new ArgumentException("Invalid file type, only PDF, DOC, and DOCX files are allowed");
            }

            var resumeText = await _resumeParserService.ExtractTextFromFileAsync(path);

            if (string.IsNullOrWhiteSpace(resumeText))
            {
                throw new InvalidOperationException("Failed to extract text from the resume.");
            }

            return resumeText;
        }
        //File Type Validation Method
        private bool IsValidFileType(string fileName)
        {
            string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".txt" };
            string fileExtension = Path.GetExtension(fileName).ToLower();
            return allowedExtensions.Contains(fileExtension);
        }
    }
}
