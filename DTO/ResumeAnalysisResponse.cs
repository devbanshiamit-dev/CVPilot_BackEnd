namespace CVPilotAPI.DTO
{
    public class ResumeAnalysisResponse
    {
        public int Score { get; set; }
        public string Profession { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = [];
        public List<string> Problems { get; set; } = [];
        public List<string> Suggestions { get; set; } = [];
    }
}
