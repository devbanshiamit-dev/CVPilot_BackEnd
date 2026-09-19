using CVPilotAPI.DTO;
using CVPilotAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CVPilotAPI.AnalysisRepository
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly string _connectionString;
        public AnalysisRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<Analysis?> GetAnalysisByResumeIdAsync(int resumeId)
        {
            const string sql = @"
            SELECT AnalysisId, Score, Profession, Experience, Skills, CreatedAt
            FROM ResumeAnalysis
            WHERE ResumeId = @ResumeId;";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ResumeId", resumeId);
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToAnalysis(reader);
            }
            return null;
        }
        public async Task<int> CreateAnalysisAsync(int resumeId, Analysis analysis)
        {
            const string sql = @"
            INSERT INTO ResumeAnalysis (ResumeId, Score, Profession, Experience, Skills)
            OUTPUT INSERTED.AnalysisId
            VALUES (@ResumeId, @Score, @Profession, @Experience, @Skills);";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@ResumeId", SqlDbType.Int).Value = resumeId;
            command.Parameters.Add("@Score", SqlDbType.Int).Value = analysis.Score;
            command.Parameters.Add("@Profession", SqlDbType.VarChar,100).Value = analysis.Profession;
            command.Parameters.Add("@Experience", SqlDbType.VarChar,50).Value = analysis.Experience;
            command.Parameters.Add("@Skills", SqlDbType.VarChar,-1).Value = string.Join(",", analysis.Skills);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }
        private Analysis MapToAnalysis(SqlDataReader reader)
        {
            return new Analysis
            {
                ResumeId = reader.GetInt32(reader.GetOrdinal("ResumeId")),
                Score = reader.GetInt32(reader.GetOrdinal("Score")),
                Profession = reader.GetString(reader.GetOrdinal("Profession")),
                Experience = reader.GetString(reader.GetOrdinal("Experience")),
                Skills = reader.GetString(reader.GetOrdinal("Skills")).Split(',').ToList(),
            };
        }
    }
}
