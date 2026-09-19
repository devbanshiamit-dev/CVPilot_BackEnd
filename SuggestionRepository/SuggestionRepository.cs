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
        public async Task<int> CreateSuggestionAsync(int analysisId, Suggestions suggestion)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO Suggestion (AnalysisId, Suggestion, Problem)
                        OUTPUT INSERTED.AnalysisId
                        VALUES (@AnalysisId, @Suggestion, @Problem)";
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@AnalysisId", SqlDbType.Int).Value = analysisId;
            command.Parameters.Add("@Suggestion", SqlDbType.VarChar).Value = string.Join(",", suggestion.Suggestion);
            command.Parameters.Add("@Problem", SqlDbType.VarChar).Value = string.Join(",", suggestion.Problem);
            return await command.ExecuteNonQueryAsync();
        }
        public async Task<Suggestions> GetSuggestionByAnalysisIdAsync(int analysisId)
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
        private Suggestions MapReaderToSuggestion(SqlDataReader reader)
        {
            var suggestion = new Suggestions
            {
                AnalysisId = reader.GetInt32(reader.GetOrdinal("AnalysisId")),
                Suggestion = reader.GetString(reader.GetOrdinal("Suggestion")),
                Problem = reader.GetString(reader.GetOrdinal("Problem"))
            };
            return suggestion;
        }
    }
}
