using SearchTutor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchTutor
{
    public class CommandHandler
    {
        private readonly DatabaseService _dbService;
        private readonly AuthService _authService;

        public CommandHandler(DatabaseService dbService, AuthService authService)
        {
            _dbService = dbService;
            _authService = authService;
        }

        public async Task<string> ProcessMessageAsync(string message, string clientAddress)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📥 Получено: '{message}' от {clientAddress}");

            if (!message.StartsWith("CMD:"))
                return "RESPONSE:ERROR:INVALID_COMMAND";

            string command = message.Substring(4);
            string[] parts = command.Split(':');
            string action = parts[0].ToUpper();

            try
            {
                switch (action)
                {
                    
                    case "GET_TEACHERS":
                        return await HandleGetTeachersAsync(parts);

                    case "ADD_TEACHER":
                        return await HandleAddTeacherAsync(parts);

                    case "GET_ALL_TEACHERS":
                        return await HandleGetAllTeachersAsync(parts);

                    case "DELETE_TEACHER":
                        return await HandleDeleteTeacherAsync(parts);

                    case "RESTORE_TEACHER":
                        return await HandleRestoreTeacherAsync(parts);

                    
                    case "GET_STUDENTS":
                        return await HandleGetStudentsAsync(parts);

                    case "REGISTER_STUDENT":
                        return await HandleRegisterStudentAsync(parts);

                    case "LOGIN":
                        return await HandleLoginAsync(parts);

                    case "DELETE_STUDENT":
                        return await HandleDeleteStudentAsync(parts);

                    case "RESTORE_STUDENT":
                        return await HandleRestoreStudentAsync(parts);

                    case "DEACTIVATE_STUDENT":
                        return await HandleDeactivateStudentAsync(parts);

                    case "ACTIVATE_STUDENT":
                        return await HandleActivateStudentAsync(parts);

                    case "GET_ALL_STUDENTS":
                        return await HandleGetAllStudentsAsync(parts);

                    case "ADD_REVIEW":
                        return await HandleAddReviewAsync(parts);

                    case "GET_SUBJECTS":
                        return await HandleGetSubjectsAsync();

                    default:
                        return "RESPONSE:ERROR:UNKNOWN_COMMAND";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка: {ex.Message}");
                return "RESPONSE:ERROR:SERVER_ERROR";
            }
        }

        private async Task<string> HandleGetTeachersAsync(string[] parts)
        {
            string subject = parts.Length > 1 && parts[1] != "-" ? parts[1] : null;
            int? experience = parts.Length > 2 && int.TryParse(parts[2], out int exp) ? exp : (int?)null;
            decimal? maxPrice = parts.Length > 3 && decimal.TryParse(parts[3], out decimal price) ? price : (decimal?)null;

            var teachers = await _dbService.GetAllTeachersAsync(subject, experience, maxPrice);
            return EncodeTeachersResponse(teachers);
        }
        private async Task<string> HandleDeleteStudentAsync(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int studentId))
                return "RESPONSE:ERROR:INVALID_ID";

            bool deleted = await _dbService.SoftDeleteStudentAsync(studentId);
            if (deleted)
            {
                Console.WriteLine($"  🗑️ Ученик помечен как удаленный: ID {studentId}");
                return $"RESPONSE:DELETED:ID={studentId}";
            }
            return "RESPONSE:ERROR:STUDENT_NOT_FOUND";
        }

        private async Task<string> HandleRestoreStudentAsync(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int studentId))
                return "RESPONSE:ERROR:INVALID_ID";

            bool restored = await _dbService.RestoreStudentAsync(studentId);
            if (restored)
            {
                Console.WriteLine($"  ↩️ Ученик восстановлен: ID {studentId}");
                return $"RESPONSE:RESTORED:ID={studentId}";
            }
            return "RESPONSE:ERROR:STUDENT_NOT_FOUND";
        }

        private async Task<string> HandleDeactivateStudentAsync(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int studentId))
                return "RESPONSE:ERROR:INVALID_ID";

            bool deactivated = await _dbService.DeactivateStudentAsync(studentId);
            if (deactivated)
            {
                Console.WriteLine($" Ученик деактивирован: ID {studentId}");
                return $"RESPONSE:DEACTIVATED:ID={studentId}";
            }
            return "RESPONSE:ERROR:STUDENT_NOT_FOUND";
        }

        private async Task<string> HandleActivateStudentAsync(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int studentId))
                return "RESPONSE:ERROR:INVALID_ID";

            bool activated = await _dbService.ActivateStudentAsync(studentId);
            if (activated)
            {
                Console.WriteLine($"  📱 Ученик активирован: ID {studentId}");
                return $"RESPONSE:ACTIVATED:ID={studentId}";
            }
            return "RESPONSE:ERROR:STUDENT_NOT_FOUND";
        }

        private async Task<string> HandleGetAllStudentsAsync(string[] parts)
        {
            bool includeDeleted = parts.Length > 1 && parts[1] == "with_deleted";
            var students = await _dbService.GetAllStudentsAsync(includeDeleted);
            return EncodeStudentsResponse(students);
        }
        private async Task<string> HandleAddTeacherAsync(string[] parts)
        {
            
            if (parts.Length < 11)
                return "RESPONSE:ERROR:INVALID_FORMAT";

            
            string adminName = parts[10];
            if (adminName != "admin")
                return "RESPONSE:ERROR:ACCESS_DENIED";

            var teacher = new Teacher
            {
                LastName = parts[1],
                FirstName = parts[2],
                MiddleName = parts[3] == "-" ? null : parts[3],
                Subject = parts[4],
                Experience = int.Parse(parts[5]),
                PriceMin = decimal.Parse(parts[6]),
                PriceMax = decimal.Parse(parts[7]),
                Education = parts[8] == "-" ? null : parts[8],
                Description = parts[9] == "-" ? null : parts[9]
            };

            int newId = await _dbService.AddTeacherAsync(teacher, adminName);
            Console.WriteLine($"  Добавлен учитель: {teacher.FullName} (ID: {newId})");
            return $"RESPONSE:ADDED:ID={newId}";
        }

        private async Task<string> HandleGetAllTeachersAsync(string[] parts)
        {
            try
            {
                Console.WriteLine("  Запрос всех учителей (админка)");

                bool includeDeleted = parts.Length > 1 && parts[1] == "with_deleted";
                var teachers = await _dbService.GetAllTeachersAsync(includeDeleted);

                Console.WriteLine($"  Найдено учителей: {teachers.Count}");
                return EncodeTeachersResponse(teachers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Ошибка: {ex.Message}");
                return "RESPONSE:ERROR:DB_ERROR";
            }
        }

        private async Task<string> HandleDeleteTeacherAsync(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int teacherId))
                return "RESPONSE:ERROR:INVALID_ID";

            bool deleted = await _dbService.SoftDeleteTeacherAsync(teacherId);
            if (deleted)
            {
                Console.WriteLine($" Учитель помечен как удаленный: ID {teacherId}");
                return $"RESPONSE:DELETED:ID={teacherId}";
            }
            return "RESPONSE:ERROR:TEACHER_NOT_FOUND";
        }

        private async Task<string> HandleRestoreTeacherAsync(string[] parts)
        {
            if (parts.Length < 2 || !int.TryParse(parts[1], out int teacherId))
                return "RESPONSE:ERROR:INVALID_ID";

            bool restored = await _dbService.RestoreTeacherAsync(teacherId);
            if (restored)
            {
                Console.WriteLine($"  ↩️ Учитель восстановлен: ID {teacherId}");
                return $"RESPONSE:RESTORED:ID={teacherId}";
            }
            return "RESPONSE:ERROR:TEACHER_NOT_FOUND";
        }

        private async Task<string> HandleGetStudentsAsync(string[] parts)
        {
            try
            {
                Console.WriteLine("   Запрос списка учеников");

                
                string subject = parts.Length > 1 && parts[1] != "-" ? parts[1] : null;

                
                var students = await _dbService.GetAllStudentsAsync(false);

                
                if (!string.IsNullOrEmpty(subject))
                {
                    students = students.Where(s =>
                        s.PreferredSubjects != null &&
                        s.PreferredSubjects.ToLower().Contains(subject.ToLower())
                    ).ToList();
                }

                Console.WriteLine($"   Найдено учеников: {students.Count}");

                return EncodeStudentsResponse(students);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Ошибка получения учеников: {ex.Message}");
                return "RESPONSE:ERROR:DB_ERROR";
            }
        }

        private async Task<string> HandleRegisterStudentAsync(string[] parts)
        {
            
            if (parts.Length < 8)
                return "RESPONSE:ERROR:INVALID_FORMAT";

            var (success, studentId, error) = await _authService.RegisterStudentAsync(
                parts[1], parts[2], parts[3] == "-" ? null : parts[3],
                parts[4], parts[5], parts[6] == "-" ? null : parts[6],
                parts[7] == "-" ? null : parts[7]
            );

            if (success)
            {
                Console.WriteLine($"  Зарегистрирован ученик ID: {studentId}");
                return $"RESPONSE:REGISTERED:ID={studentId}";
            }
            else
            {
                return $"RESPONSE:ERROR:{error}";
            }
        }

        private async Task<string> HandleLoginAsync(string[] parts)
        {
            
            if (parts.Length < 3)
                return "RESPONSE:ERROR:INVALID_FORMAT";

            var (success, userId, role, error) = await _authService.LoginAsync(parts[1], parts[2]);

            if (success)
            {
                Console.WriteLine($"   Вход: {role} ID: {userId}");
                return $"RESPONSE:LOGIN_SUCCESS:ID={userId}:ROLE={role}";
            }
            else
            {
                return $"RESPONSE:ERROR:{error}";
            }
        }

        private async Task<string> HandleAddReviewAsync(string[] parts)
        {
            
            if (parts.Length < 5)
                return "RESPONSE:ERROR:INVALID_FORMAT";

            int studentId = int.Parse(parts[1]);
            int teacherId = int.Parse(parts[2]);
            int rating = int.Parse(parts[3]);
            string comment = parts[4].Replace("[PIPE]", "|").Replace("[S]", ";");

            bool added = await _dbService.AddReviewFromStudentAsync(studentId, teacherId, rating, comment);

            if (added)
            {
                Console.WriteLine($"  ⭐ Добавлен отзыв от ученика {studentId} учителю {teacherId}");
                return "RESPONSE:REVIEW_ADDED";
            }
            else
            {
                return "RESPONSE:ERROR:REVIEW_EXISTS";
            }
        }

        private async Task<string> HandleGetSubjectsAsync()
        {
            var subjects = await _dbService.GetAllSubjectsAsync();
            return EncodeSubjectsResponse(subjects);
        }

        private string EncodeTeachersResponse(List<Teacher> teachers)
        {
            var sb = new StringBuilder("RESPONSE:TEACHERS:");
            foreach (var t in teachers)
            {
                sb.Append($"{t.Id}|{Escape(t.LastName)}|{Escape(t.FirstName)}|{Escape(t.MiddleName)}|" +
                         $"{Escape(t.Subject)}|{t.Experience}|{t.PriceMin}|{t.PriceMax}|" +
                         $"{Escape(t.Education)}|{Escape(t.Description)}|{t.Rating:F1}|{t.ReviewsCount};");
            }
            return sb.ToString();
        }

        private string EncodeStudentsResponse(List<Student> students)
        {
            var sb = new StringBuilder("RESPONSE:STUDENTS:");
            foreach (var s in students)
            {
                sb.Append($"{s.Id}|{Escape(s.LastName)}|{Escape(s.FirstName)}|{Escape(s.MiddleName)}|" +
                         $"{Escape(s.Email)}|{Escape(s.Goals)}|{Escape(s.PreferredSubjects)}|" +
                         $"{s.Rating:F1}|{s.ReviewsCount};");
            }
            return sb.ToString();
        }

        private string EncodeSubjectsResponse(List<Subject> subjects)
        {
            var sb = new StringBuilder("RESPONSE:SUBJECTS:");
            foreach (var s in subjects)
            {
                sb.Append($"{s.Id}|{Escape(s.Name)}|{Escape(s.Category)};");
            }
            return sb.ToString();
        }

        private string Escape(string text)
        {
            return text?.Replace("|", "[PIPE]").Replace(";", "[S]") ?? "-";
        }
    }
}
