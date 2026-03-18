using Microsoft.Data.SqlClient;
using SearchTutor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchTutor
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Инициализация базы данных
        public async Task InitializeDatabaseAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine(" Подключение к БД успешно");

                // Таблица учителей
                var createTeachers = connection.CreateCommand();
                createTeachers.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Teachers' AND xtype='U')
                    CREATE TABLE Teachers (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        LastName NVARCHAR(50) NOT NULL,
                        FirstName NVARCHAR(50) NOT NULL,
                        MiddleName NVARCHAR(50),
                        Subject NVARCHAR(100) NOT NULL,
                        Experience INT NOT NULL,
                        PriceMin DECIMAL(10,2) NOT NULL,
                        PriceMax DECIMAL(10,2) NOT NULL,
                        Education NVARCHAR(MAX),
                        Description NVARCHAR(MAX),
                        Rating FLOAT DEFAULT 0,
                        ReviewsCount INT DEFAULT 0,
                        AddedByAdmin NVARCHAR(50),
                        IsDeleted BIT DEFAULT 0,
                        CreatedAt DATETIME DEFAULT GETDATE()
                    )";
                await createTeachers.ExecuteNonQueryAsync();
                Console.WriteLine(" Таблица Teachers создана");

                // Таблица учеников
                var createStudents = connection.CreateCommand();
                createStudents.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Students' AND xtype='U')
                    CREATE TABLE Students (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        LastName NVARCHAR(50) NOT NULL,
                        FirstName NVARCHAR(50) NOT NULL,
                        MiddleName NVARCHAR(50),
                        Email NVARCHAR(100) UNIQUE NOT NULL,
                        PasswordHash NVARCHAR(256) NOT NULL,
                        Goals NVARCHAR(MAX),
                        PreferredSubjects NVARCHAR(MAX),
                        Rating FLOAT DEFAULT 0,
                        ReviewsCount INT DEFAULT 0,
                        IsActive BIT DEFAULT 1,
                        IsDeleted BIT DEFAULT 0,
                        CreatedAt DATETIME DEFAULT GETDATE(),
                        LastLoginAt DATETIME
                    )";
                await createStudents.ExecuteNonQueryAsync();
                Console.WriteLine(" Таблица Students создана");

                // Таблица отзывов
                var createReviews = connection.CreateCommand();
                createReviews.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reviews' AND xtype='U')
                    CREATE TABLE Reviews (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FromStudentId INT NULL,
                        ToTeacherId INT NULL,
                        FromTeacherId INT NULL,
                        ToStudentId INT NULL,
                        Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
                        Comment NVARCHAR(MAX) NOT NULL,
                        IsModerated BIT DEFAULT 0,
                        IsDeleted BIT DEFAULT 0,
                        CreatedAt DATETIME DEFAULT GETDATE(),
                        FOREIGN KEY (FromStudentId) REFERENCES Students(Id),
                        FOREIGN KEY (ToTeacherId) REFERENCES Teachers(Id)
                    )";
                await createReviews.ExecuteNonQueryAsync();
                Console.WriteLine(" Таблица Reviews создана");

                // Таблица предметов
                var createSubjects = connection.CreateCommand();
                createSubjects.CommandText = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Subjects' AND xtype='U')
                    CREATE TABLE Subjects (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL UNIQUE,
                        Category NVARCHAR(50)
                    )";
                await createSubjects.ExecuteNonQueryAsync();
                Console.WriteLine("Таблица Subjects создана");

                // Заполняем предметами, если пусто
                await SeedSubjectsAsync(connection);
            }
        }

        private async Task SeedSubjectsAsync(SqlConnection connection)
        {
            var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Subjects", connection);
            int count = (int)await checkCommand.ExecuteScalarAsync();

            if (count == 0)
            {
                string[] subjects = {
                    "Математика", "Физика", "Химия", "Биология", "Русский язык",
                    "Литература", "История", "Обществознание", "География",
                    "Английский язык", "Немецкий язык", "Французский язык",
                    "Информатика", "Программирование", "Высшая математика"
                };

                foreach (var subject in subjects)
                {
                    var insertCommand = new SqlCommand(
                        "INSERT INTO Subjects (Name, Category) VALUES (@Name, 'Общее')",
                        connection);
                    insertCommand.Parameters.AddWithValue("@Name", subject);
                    await insertCommand.ExecuteNonQueryAsync();
                }
                Console.WriteLine("Таблица Subjects заполнена");
            }
        }

        // 

        public async Task<List<Teacher>> GetAllTeachersAsync(string subject = null, int? minExperience = null, decimal? maxPrice = null)
        {
            var teachers = new List<Teacher>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT Id, LastName, FirstName, MiddleName, Subject, Experience,
                           PriceMin, PriceMax, Education, Description, Rating, ReviewsCount,
                           AddedByAdmin, CreatedAt
                    FROM Teachers 
                    WHERE IsDeleted = 0";

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(subject))
                {
                    sql += " AND Subject LIKE @Subject";
                    parameters.Add(new SqlParameter("@Subject", $"%{subject}%"));
                }

                if (minExperience.HasValue)
                {
                    sql += " AND Experience >= @Experience";
                    parameters.Add(new SqlParameter("@Experience", minExperience.Value));
                }

                if (maxPrice.HasValue)
                {
                    sql += " AND PriceMin <= @MaxPrice";
                    parameters.Add(new SqlParameter("@MaxPrice", maxPrice.Value));
                }

                sql += " ORDER BY Rating DESC, ReviewsCount DESC";

                var command = new SqlCommand(sql, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        teachers.Add(new Teacher
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Subject = reader.GetString(4),
                            Experience = reader.GetInt32(5),
                            PriceMin = reader.GetDecimal(6),
                            PriceMax = reader.GetDecimal(7),
                            Education = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Rating = reader.GetDouble(10),
                            ReviewsCount = reader.GetInt32(11),
                            AddedByAdmin = reader.IsDBNull(12) ? null : reader.GetString(12),
                            CreatedAt = reader.GetDateTime(13)
                        });
                    }
                }
            }
            return teachers;
        }

        public async Task<int> AddTeacherAsync(Teacher teacher, string adminName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Teachers (
                        LastName, FirstName, MiddleName, Subject, Experience,
                        PriceMin, PriceMax, Education, Description, AddedByAdmin, IsDeleted
                    ) OUTPUT INSERTED.Id VALUES (
                        @LastName, @FirstName, @MiddleName, @Subject, @Experience,
                        @PriceMin, @PriceMax, @Education, @Description, @AdminName, 0
                    )";

                command.Parameters.AddWithValue("@LastName", teacher.LastName);
                command.Parameters.AddWithValue("@FirstName", teacher.FirstName);
                command.Parameters.AddWithValue("@MiddleName", teacher.MiddleName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Subject", teacher.Subject);
                command.Parameters.AddWithValue("@Experience", teacher.Experience);
                command.Parameters.AddWithValue("@PriceMin", teacher.PriceMin);
                command.Parameters.AddWithValue("@PriceMax", teacher.PriceMax);
                command.Parameters.AddWithValue("@Education", teacher.Education ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", teacher.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AdminName", adminName);

                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task<List<Student>> GetAllStudentsAsync(string subject = null)
        {
            var students = new List<Student>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT Id, LastName, FirstName, MiddleName, Email, Goals,
                           PreferredSubjects, Rating, ReviewsCount, IsActive, CreatedAt
                    FROM Students 
                    WHERE IsDeleted = 0 AND IsActive = 1";

                if (!string.IsNullOrEmpty(subject))
                {
                    sql += " AND PreferredSubjects LIKE @Subject";
                }

                sql += " ORDER BY Rating DESC, ReviewsCount DESC";

                var command = new SqlCommand(sql, connection);
                if (!string.IsNullOrEmpty(subject))
                {
                    command.Parameters.AddWithValue("@Subject", $"%{subject}%");
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Email = reader.GetString(4),
                            Goals = reader.IsDBNull(5) ? null : reader.GetString(5),
                            PreferredSubjects = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Rating = reader.GetDouble(7),
                            ReviewsCount = reader.GetInt32(8),
                            IsActive = reader.GetBoolean(9),
                            CreatedAt = reader.GetDateTime(10)
                        });
                    }
                }
            }
            return students;
        }
        public async Task<List<Teacher>> GetAllTeachersAsync(bool includeDeleted = false)
        {
            var teachers = new List<Teacher>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
            SELECT Id, LastName, FirstName, MiddleName, Subject, Experience,
                   PriceMin, PriceMax, Education, Description, Rating, ReviewsCount,
                   AddedByAdmin, IsDeleted, CreatedAt
            FROM Teachers 
            WHERE 1=1";

                if (!includeDeleted)
                {
                    sql += " AND IsDeleted = 0";
                }

                sql += " ORDER BY IsDeleted, Rating DESC";

                var command = new SqlCommand(sql, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        teachers.Add(new Teacher
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Subject = reader.GetString(4),
                            Experience = reader.GetInt32(5),
                            PriceMin = reader.GetDecimal(6),
                            PriceMax = reader.GetDecimal(7),
                            Education = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Rating = reader.GetDouble(10),
                            ReviewsCount = reader.GetInt32(11),
                            AddedByAdmin = reader.IsDBNull(12) ? null : reader.GetString(12),
                            IsDeleted = reader.GetBoolean(13),
                            CreatedAt = reader.GetDateTime(14)
                        });
                    }
                }
            }
            return teachers;
        }
        public async Task<bool> SoftDeleteTeacherAsync(int teacherId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE Teachers 
            SET IsDeleted = 1
            WHERE Id = @TeacherId AND IsDeleted = 0";

                command.Parameters.AddWithValue("@TeacherId", teacherId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }
        public async Task<bool> RestoreTeacherAsync(int teacherId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE Teachers 
            SET IsDeleted = 0
            WHERE Id = @TeacherId AND IsDeleted = 1";

                command.Parameters.AddWithValue("@TeacherId", teacherId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // ========== ОТЗЫВЫ ==========

        public async Task<bool> AddReviewFromStudentAsync(int studentId, int teacherId, int rating, string comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Проверка, не оставлял ли уже ученик отзыв этому учителю
                        var checkCommand = connection.CreateCommand();
                        checkCommand.Transaction = transaction;
                        checkCommand.CommandText = @"
                            SELECT COUNT(*) FROM Reviews 
                            WHERE FromStudentId = @StudentId AND ToTeacherId = @TeacherId AND IsDeleted = 0";
                        checkCommand.Parameters.AddWithValue("@StudentId", studentId);
                        checkCommand.Parameters.AddWithValue("@TeacherId", teacherId);

                        int exists = (int)await checkCommand.ExecuteScalarAsync();
                        if (exists > 0)
                            return false;

                        // Добавляем отзыв
                        var reviewCommand = connection.CreateCommand();
                        reviewCommand.Transaction = transaction;
                        reviewCommand.CommandText = @"
                            INSERT INTO Reviews (FromStudentId, ToTeacherId, Rating, Comment, IsModerated, IsDeleted)
                            VALUES (@StudentId, @TeacherId, @Rating, @Comment, 1, 0)";

                        reviewCommand.Parameters.AddWithValue("@StudentId", studentId);
                        reviewCommand.Parameters.AddWithValue("@TeacherId", teacherId);
                        reviewCommand.Parameters.AddWithValue("@Rating", rating);
                        reviewCommand.Parameters.AddWithValue("@Comment", comment);

                        await reviewCommand.ExecuteNonQueryAsync();

                        // Обновляем рейтинг учителя
                        await UpdateTeacherRatingAsync(connection, transaction, teacherId);

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        // Мягкое удаление ученика
        public async Task<bool> SoftDeleteStudentAsync(int studentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE Students 
            SET IsDeleted = 1, IsActive = 0
            WHERE Id = @StudentId AND IsDeleted = 0";

                command.Parameters.AddWithValue("@StudentId", studentId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // Восстановление ученика
        public async Task<bool> RestoreStudentAsync(int studentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE Students 
            SET IsDeleted = 0, IsActive = 1
            WHERE Id = @StudentId AND IsDeleted = 1";

                command.Parameters.AddWithValue("@StudentId", studentId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // Деактивация ученика (без удаления)
        public async Task<bool> DeactivateStudentAsync(int studentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE Students 
            SET IsActive = 0
            WHERE Id = @StudentId AND IsDeleted = 0";

                command.Parameters.AddWithValue("@StudentId", studentId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // Активация ученика
        public async Task<bool> ActivateStudentAsync(int studentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE Students 
            SET IsActive = 1
            WHERE Id = @StudentId AND IsDeleted = 0";

                command.Parameters.AddWithValue("@StudentId", studentId);
                int rows = await command.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // Получение учеников с учетом фильтра удаленных
        public async Task<List<Student>> GetAllStudentsAsync(bool includeDeleted = false)
        {
            var students = new List<Student>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
            SELECT Id, LastName, FirstName, MiddleName, Email, Goals,
                   PreferredSubjects, Rating, ReviewsCount, IsActive, IsDeleted, CreatedAt
            FROM Students 
            WHERE 1=1";

                if (!includeDeleted)
                {
                    sql += " AND IsDeleted = 0";
                }

                sql += " ORDER BY IsDeleted, Rating DESC";

                var command = new SqlCommand(sql, connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        students.Add(new Student
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Email = reader.GetString(4),
                            Goals = reader.IsDBNull(5) ? null : reader.GetString(5),
                            PreferredSubjects = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Rating = reader.GetDouble(7),
                            ReviewsCount = reader.GetInt32(8),
                            IsActive = reader.GetBoolean(9),
                            IsDeleted = reader.GetBoolean(10),
                            CreatedAt = reader.GetDateTime(11)
                        });
                    }
                }
            }
            return students;
        }

        private async Task UpdateTeacherRatingAsync(SqlConnection connection, SqlTransaction transaction, int teacherId)
        {
            var ratingCommand = connection.CreateCommand();
            ratingCommand.Transaction = transaction;
            ratingCommand.CommandText = @"
                UPDATE Teachers 
                SET Rating = (SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews WHERE ToTeacherId = @TeacherId AND IsDeleted = 0),
                    ReviewsCount = (SELECT COUNT(*) FROM Reviews WHERE ToTeacherId = @TeacherId AND IsDeleted = 0)
                WHERE Id = @TeacherId";
            ratingCommand.Parameters.AddWithValue("@TeacherId", teacherId);
            await ratingCommand.ExecuteNonQueryAsync();
        }

        // ========== ПРЕДМЕТЫ ==========

        public async Task<List<Subject>> GetAllSubjectsAsync()
        {
            var subjects = new List<Subject>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT Id, Name, Category FROM Subjects ORDER BY Name", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        subjects.Add(new Subject
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Category = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return subjects;
        }
    }
}
