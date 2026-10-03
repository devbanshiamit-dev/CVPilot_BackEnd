namespace CVPilotAPI.Models
{
    public class UserAnalysis
    {
        public int ResumeId { get; set; }
        public int UserId { get; set; }
        public int AnalysisCount { get; set; }
        public DateTime WindowStartedAt { get; set; }
    }
}
