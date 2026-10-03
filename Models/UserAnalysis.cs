namespace CVPilotAPI.Models
{
    public class UserAnalysis
    {
        public int UserId { get; set; }
        public int AnalysisCount { get; set; }
        public DateTime WindowStartedAt { get; set; }
    }
}
