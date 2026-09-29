using CVPilotAPI.Models;

namespace CVPilotAPI.DTO
{
    public class DBAnalysisResponse
    {
        public required Analysis analysis { get; set; }
        public Suggestions suggestions { get; set; }
        public Problem problem { get; set; }
    }
}
