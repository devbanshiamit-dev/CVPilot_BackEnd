namespace CVPilotAPI.Models
{
    public class Resumes
    {
        public int ResumeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string? ExtractedText { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
