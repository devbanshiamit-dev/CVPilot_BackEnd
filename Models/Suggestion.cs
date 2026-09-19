namespace CVPilotAPI.Models
{
    public class Suggestion
    {
        public int AnalysisId { get; set; }
        public List<string> Suggestions { get; set; } = new();
        public List<string> Problems { get; set; } = new();
    }
}
