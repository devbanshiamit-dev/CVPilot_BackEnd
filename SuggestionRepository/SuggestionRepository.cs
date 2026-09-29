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
            var query = @"INSERT INTO Suggestion (AnalysisId, Suggestion)
                        OUTPUT INSERTED.AnalysisId
                        VALUES (@AnalysisId, @Suggestion)";
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@AnalysisId", SqlDbType.Int).Value = analysisId;
            command.Parameters.Add("@Suggestion", SqlDbType.VarChar,-1).Value = suggestion.Suggestion;
            return await command.ExecuteNonQueryAsync();
        }
        public async Task<int> CreateProblemAsync(int analysisId, Problem problem)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"INSERT INTO Problem (AnalysisId, Problem)
                        OUTPUT INSERTED.AnalysisId
                        VALUES (@AnalysisId, @Problem)";
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@AnalysisId", SqlDbType.Int).Value = analysisId;
            command.Parameters.Add("@Problem", SqlDbType.VarChar,-1).Value = problem.problem;
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
        public async Task<Problem> GetProblemByAnalysisIdAsync(int analysisId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM Problem WHERE AnalysisId = @AnalysisId";
            using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@AnalysisId", SqlDbType.Int).Value = analysisId;
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapReaderToProblem(reader);
            }
            return null!;
        }
        private Suggestions MapReaderToSuggestion(SqlDataReader reader)
        {
            var suggestion = new Suggestions
            {
                AnalysisId = reader.GetInt32(reader.GetOrdinal("AnalysisId")),
                Suggestion = reader.GetString(reader.GetOrdinal("Suggestion"))
            };
            return suggestion;
        }
        private Problem MapReaderToProblem(SqlDataReader reader)
        {
            return new Problem
            {
                AnalysisId = reader.GetInt32(reader.GetOrdinal("AnalysisId")),
                problem = reader.GetString(reader.GetOrdinal("Problem"))
            };
        }
    }
}
