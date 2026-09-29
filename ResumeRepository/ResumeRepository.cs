using CVPilotAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CVPilotAPI.Repository
{
    public class ResumeRepository : IResumeRepository
    {
        private readonly string _connectionString;

        public ResumeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ========== CREATE ==========
        public async Task<int> CreateResumeAsync(Resumes resume)
        {
            const string sql = @"
            INSERT INTO Resumes (FileName, FileType, FilePath, ExtractedText)
            OUTPUT INSERTED.ResumeId
            VALUES (@FileName, @FileType, @FilePath, @ExtractedText);";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@FileName", SqlDbType.VarChar,255).Value = resume.FileName;
            command.Parameters.Add("@FileType", SqlDbType.VarChar,50).Value = resume.FileType;
            command.Parameters.Add("@FilePath", SqlDbType.VarChar,500).Value = (object?)resume.FilePath ?? DBNull.Value;
            command.Parameters.Add("@ExtractedText", SqlDbType.VarChar, -1).Value = (object?)resume.ExtractedText ?? DBNull.Value;

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ========== READ BY ID ==========
        public async Task<Resumes?> GetResumeByIdAsync(int resumeId)
        {
            const string sql = @"
            SELECT ResumeId, FileName, FileType, FilePath, ExtractedText, UploadedAt
            FROM Resumes
            WHERE ResumeId = @ResumeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ResumeId", resumeId);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapToResume(reader);
            }

            return null;
        }

        // ========== UPDATE ==========
        public async Task<bool> UpdateAsync(Resumes resume)
        {
            const string sql = @"
            UPDATE Resumes
            SET FileName = @FileName,
                FileType = @FileType,
                FilePath = @FilePath,
                ExtractedText = @ExtractedText
            WHERE ResumeId = @ResumeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@ResumeId", resume.ResumeId);
            command.Parameters.AddWithValue("@FileName", resume.FileName);
            command.Parameters.AddWithValue("@FileType", resume.FileType);
            command.Parameters.AddWithValue("@FilePath", (object?)resume.FilePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExtractedText", (object?)resume.ExtractedText ?? DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ========== DELETE ==========
        public async Task<bool> DeleteAsync(int resumeId)
        {
            const string sql = "DELETE FROM Resumes WHERE ResumeId = @ResumeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ResumeId", resumeId);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ========== Private Mapper ==========
        private static Resumes MapToResume(SqlDataReader reader)
        {
            return new Resumes
            {
                ResumeId = reader.GetInt32(reader.GetOrdinal("ResumeId")),
                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                FileType = reader.GetString(reader.GetOrdinal("FileType")),
                FilePath = reader.IsDBNull(reader.GetOrdinal("FilePath"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("FilePath")),
                ExtractedText = reader.IsDBNull(reader.GetOrdinal("ExtractedText"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ExtractedText")),
                UploadedAt = reader.GetDateTime(reader.GetOrdinal("UploadedAt"))
            };
        }
    }
}
