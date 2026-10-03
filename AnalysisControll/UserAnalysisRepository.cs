using CVPilotAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CVPilotAPI.AnalysisControll
{
    public class UserAnalysisRepository : IUserAnalysisRepository
    {
        private readonly string _connectionString;
        public UserAnalysisRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }
        public async Task<int> CreateUserAnalysisAsync(UserAnalysis analysis)
        {
            const string sql = @"
            INSERT INTO UserAnalysis (ResumeId, UserId, AnalysisCount, WindowStartedAt)
            OUTPUT INSERTED.Id
            VALUES (@ResumeId, @UserId, @AnalysisCount, @WindowStartedAt);";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@ResumeId", SqlDbType.Int).Value = analysis.ResumeId;
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = analysis.UserId;
            command.Parameters.Add("@AnalysisCount", SqlDbType.Int).Value = analysis.AnalysisCount;
            command.Parameters.Add("@WindowStartedAt", SqlDbType.DateTime2).Value = analysis.WindowStartedAt;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }
        public async Task<UserAnalysis?> GetUserAnalysisByIdAsync(int id)
        {
            const string sql = @"
            SELECT ResumeId, UserId, AnalysisCount, WindowStartedAt
            FROM UserAnalysis
            WHERE UserId = @UserId;";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = id;
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapToUserAnalysis(reader);
            }
            return null;
        }
        public async Task UpdateUserAnalysisAsync(UserAnalysis analysis)
        {
            const string sql = @"
            UPDATE UserAnalysis
            SET AnalysisCount = @AnalysisCount, WindowStartedAt = @WindowStartedAt
            WHERE UserId = @UserId;";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@UserId", SqlDbType.Int).Value = analysis.UserId;
            command.Parameters.Add("@AnalysisCount", SqlDbType.Int).Value = analysis.AnalysisCount;
            command.Parameters.Add("@WindowStartedAt", SqlDbType.DateTime2).Value = analysis.WindowStartedAt;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        private UserAnalysis MapToUserAnalysis(SqlDataReader reader)
        {
            return new UserAnalysis
            {
                ResumeId = reader.GetInt32(reader.GetOrdinal("ResumeId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                AnalysisCount = reader.GetInt32(reader.GetOrdinal("AnalysisCount")),
                WindowStartedAt = reader.GetDateTime(reader.GetOrdinal("WindowStartedAt"))
            };
        }
    }
}
