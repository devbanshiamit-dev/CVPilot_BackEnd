using CVPilotAPI.Models;
using Microsoft.Data.SqlClient;

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
        public async Task<int> CreateAsync(Resume resume)
        {
            const string sql = @"
            INSERT INTO Resume (FileName, FileType, FileUrl, ExtractedText)
            OUTPUT INSERTED.ResumeId
            VALUES (@FileName, @FileType, @FileUrl, @ExtractedText);";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@FileName", resume.FileName);
            command.Parameters.AddWithValue("@FileType", resume.FileType);
            command.Parameters.AddWithValue("@FileUrl", (object?)resume.FileUrl ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExtractedText", (object?)resume.ExtractedText ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ========== READ BY ID ==========
        public async Task<Resume?> GetByIdAsync(int resumeId)
        {
            const string sql = @"
            SELECT ResumeId, FileName, FileType, FileUrl, ExtractedText, CreatedAt
            FROM Resume
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

        // ========== READ ALL ==========
        public async Task<IEnumerable<Resume>> GetAllAsync()
        {
            const string sql = @"
            SELECT ResumeId, FileName, FileType, FileUrl, ExtractedText, CreatedAt
            FROM Resume
            ORDER BY CreatedAt DESC;";

            var resumes = new List<Resume>();

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resumes.Add(MapToResume(reader));
            }

            return resumes;
        }

        // ========== UPDATE ==========
        public async Task<bool> UpdateAsync(Resume resume)
        {
            const string sql = @"
            UPDATE Resume
            SET FileName = @FileName,
                FileType = @FileType,
                FileUrl = @FileUrl,
                ExtractedText = @ExtractedText
            WHERE ResumeId = @ResumeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@ResumeId", resume.ResumeId);
            command.Parameters.AddWithValue("@FileName", resume.FileName);
            command.Parameters.AddWithValue("@FileType", resume.FileType);
            command.Parameters.AddWithValue("@FileUrl", (object?)resume.FileUrl ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExtractedText", (object?)resume.ExtractedText ?? DBNull.Value);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ========== DELETE ==========
        public async Task<bool> DeleteAsync(int resumeId)
        {
            const string sql = "DELETE FROM Resume WHERE ResumeId = @ResumeId;";

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ResumeId", resumeId);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ========== Private Mapper ==========
        private static Resume MapToResume(SqlDataReader reader)
        {
            return new Resume
            {
                ResumeId = reader.GetInt32(reader.GetOrdinal("ResumeId")),
                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                FileType = reader.GetString(reader.GetOrdinal("FileType")),
                FileUrl = reader.IsDBNull(reader.GetOrdinal("FileUrl"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("FileUrl")),
                ExtractedText = reader.IsDBNull(reader.GetOrdinal("ExtractedText"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ExtractedText")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
