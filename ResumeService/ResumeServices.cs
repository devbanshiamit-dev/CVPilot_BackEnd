using CVPilotAPI.DTO;
using CVPilotAPI.ResumeAnalyze;
using CVPilotAPI.TextExtractionEngine;

namespace CVPilotAPI.ResumeService
{
    public class ResumeServices : IResumeServices
    {
        private readonly IResumeParserService _resumeParserService;
        private readonly IResumeAnalyze _resumeAnalysisService;
        public ResumeServices(IResumeParserService resumeParserService, IResumeAnalyze resumeAnalysisService)
        {
            _resumeParserService = resumeParserService;
            _resumeAnalysisService = resumeAnalysisService;
        }

        //File Download Methods
        public async Task<byte[]> DownloadResumeAsync(string fileName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);
            byte[] fileBytes = await File.ReadAllBytesAsync(path);
            return fileBytes;
        }

        public async Task<ResumeAnalysisResponse> AnalysisFileAsync(IFormFile file)
        {
            var uploadedFileName = await UploadResumeAsync(file);

            var extractedText = await ExtractTextAsync(uploadedFileName);

            var analysisRequest = new ResumeAnalysisRequest
            {
                ExtractedText = extractedText
            };
            return await _resumeAnalysisService.AnalyzeResumeAsync(analysisRequest);
        }
        //file Upload Methods
        private async Task<string> UploadResumeAsync(IFormFile file)
        {
            if (!IsValidFileType(file.FileName))
            {
                throw new ArgumentException("Invalid file type only txt, PDF, DOC, and DOCX files are allowed");
            }

            var filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var path = Path.Combine(uploadFolder, filename);

            using FileStream stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return filename;
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
