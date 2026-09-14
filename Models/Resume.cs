namespace CVPilotAPI.Models
{
    public class Resume
    {
        public int ResumeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? ExtractedText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
