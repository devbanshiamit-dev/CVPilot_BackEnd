namespace CVPilotAPI.Models
{
    public class Suggestions
    {
        public int AnalysisId { get; set; }
        public string Suggestion { get; set; } = string.Empty;
        public string Problem { get; set; } = string.Empty;
    }
}
