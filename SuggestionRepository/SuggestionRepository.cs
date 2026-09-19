using CVPilotAPI.Models;
using CVPilotAPI.SuggestionRepository;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CVPilotAPI.SuggetionRepository
{
    public class SuggestionRepository : ISuggestionRepository
    {
        private readonly string _connectionString;
        public SuggestionRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }
        public async Task<int> CreateSuggestionAsync(int analysisId, Suggestion suggestion)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO Suggestion (AnalysisId, Suggestion, Problem)
                        OUTPUT INSERTED.AnalysisId
                        VALUES (@AnalysisId, @Suggestion, @Problem)";
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@AnalysisId", SqlDbType.Int).Value = analysisId;
            command.Parameters.Add("@Suggestion", SqlDbType.VarChar).Value = string.Join(",", suggestion.Suggestions);
            command.Parameters.Add("@Problem", SqlDbType.VarChar).Value = string.Join(",", suggestion.Problems);
            return await command.ExecuteNonQueryAsync();
        }
        public async Task<Suggestion> GetSuggestionByAnalysisIdAsync(int analysisId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT * FROM Suggestion WHERE AnalysisId = @AnalysisId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@AnalysisId", SqlDbType.Int).Value = analysisId;
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToSuggestion(reader);
            }
            return null!;
        }
        private Suggestion MapReaderToSuggestion(SqlDataReader reader)
        {
            var suggestion = new Suggestion
            {
                AnalysisId = reader.GetInt32(reader.GetOrdinal("AnalysisId")),
                Suggestions = reader.IsDBNull(reader.GetOrdinal("Suggestions"))
                    ? new List<string>()
                    : reader.GetString(reader.GetOrdinal("Suggestions")).Split(',').ToList(),
                Problems = reader.IsDBNull(reader.GetOrdinal("Problems"))
                    ? new List<string>()
                    : reader.GetString(reader.GetOrdinal("Problems")).Split(',').ToList()
            };
            return suggestion;
        }
    }
}
