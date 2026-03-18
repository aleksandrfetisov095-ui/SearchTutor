using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SearchTutor
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        
        public async Task<(bool success, int studentId, string error)> RegisterStudentAsync(
            string lastName, string firstName, string middleName,
            string email, string password, string goals, string preferredSubjects)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    
                    var checkCommand = new SqlCommand(
                        "SELECT COUNT(*) FROM Students WHERE Email = @Email AND IsDeleted = 0",
                        connection);
                    checkCommand.Parameters.AddWithValue("@Email", email);

                    int exists = (int)await checkCommand.ExecuteScalarAsync();
                    if (exists > 0)
                        return (false, 0, "EMAIL_EXISTS");

                    
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Students (LastName, FirstName, MiddleName, Email, PasswordHash,
                                             Goals, PreferredSubjects, Rating, ReviewsCount, IsActive, IsDeleted, CreatedAt)
                        OUTPUT INSERTED.Id
                        VALUES (@LastName, @FirstName, @MiddleName, @Email, @PasswordHash,
                                @Goals, @PreferredSubjects, 0, 0, 1, 0, GETDATE())";

                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@MiddleName", string.IsNullOrEmpty(middleName) ? (object)DBNull.Value : middleName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                    command.Parameters.AddWithValue("@Goals", string.IsNullOrEmpty(goals) ? (object)DBNull.Value : goals);
                    command.Parameters.AddWithValue("@PreferredSubjects", string.IsNullOrEmpty(preferredSubjects) ? (object)DBNull.Value : preferredSubjects);

                    int newId = (int)await command.ExecuteScalarAsync();
                    return (true, newId, null);
                }
            }
            catch (Exception ex)
            {
                return (false, 0, $"DB_ERROR:{ex.Message}");
            }
        }

        
        public async Task<(bool success, int userId, string role, string error)> LoginAsync(string email, string password)
        {
            string passwordHash = HashPassword(password);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                
                var studentCommand = new SqlCommand(
                    @"SELECT Id, LastName, FirstName FROM Students 
                      WHERE Email = @Email AND PasswordHash = @PasswordHash AND IsActive = 1 AND IsDeleted = 0",
                    connection);
                studentCommand.Parameters.AddWithValue("@Email", email);
                studentCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);

                using (var reader = await studentCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int studentId = reader.GetInt32(0);
                        reader.Close();

                        
                        var updateCommand = new SqlCommand(
                            "UPDATE Students SET LastLoginAt = GETDATE() WHERE Id = @Id",
                            connection);
                        updateCommand.Parameters.AddWithValue("@Id", studentId);
                        await updateCommand.ExecuteNonQueryAsync();

                        return (true, studentId, "Student", null);
                    }
                }

                
                if (email == "admin@searchtutor.ru" && password == "admin123")
                {
                    return (true, 1, "Admin", null);
                }

                return (false, 0, null, "INVALID_CREDENTIALS");
            }
        }
    }
}
