namespace CVPilotAPI.Models
{
    public class Analysis
    {
        public int ResumeId { get; set; }
        public int Score { get; set; }
        public string Profession { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
    }
}
