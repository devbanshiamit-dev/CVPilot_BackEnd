namespace CVPilotAPI.Models
{
    public class Analysis
    {
        public int ResumeId { get; set; }
        public int Score { get; set; }
        public string Profession { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public List<string> Problems { get; set; } = new();
    }
}
