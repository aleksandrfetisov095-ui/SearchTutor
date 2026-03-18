using ClientTutor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientTutor.Services
{
    public class TutorClientService
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly IPEndPoint _serverEndPoint;

        public TutorClientService(string serverIp, int serverPort)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        }
        private async Task<string> SendAndReceiveAsync(string message)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                
                client.ReceiveTimeout = 3000;

                byte[] sendData = Encoding.UTF8.GetBytes(message);

                
                await Task.Run(() => client.SendTo(sendData, _serverEndPoint));

                
                byte[] receiveBuffer = new byte[16384]; 
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    var receiveTask = Task.Run(() => client.ReceiveFrom(receiveBuffer, ref remoteEndPoint));
                    if (await Task.WhenAny(receiveTask, Task.Delay(5000)) == receiveTask)
                    {
                        int received = receiveTask.Result;
                        return Encoding.UTF8.GetString(receiveBuffer, 0, received);
                    }
                    return null; 
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Ошибка сокета: {ex.Message}");
                    return null;
                }
            }
        }

        
        private string Escape(string text)
        {
            return text?.Replace("|", "[PIPE]").Replace(";", "[S]") ?? "-";
        }

        private string Unescape(string text)
        {
            if (text == "-" || text == null) return null;
            return text.Replace("[PIPE]", "|").Replace("[S]", ";");
        }

        
        public async Task<List<Teacher>> GetTeachersAsync(string subject = null, int? minExperience = null, decimal? maxPrice = null)
        {
            string command = $"CMD:GET_TEACHERS:{subject ?? "-"}:{minExperience?.ToString() ?? "-"}:{maxPrice?.ToString() ?? "-"}";
            string response = await SendAndReceiveAsync(command);

            var teachers = new List<Teacher>();

            if (response?.StartsWith("RESPONSE:TEACHERS:") == true)
            {
                string data = response.Replace("RESPONSE:TEACHERS:", "");
                string[] items = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    string[] parts = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 12)
                    {
                        teachers.Add(new Teacher
                        {
                            Id = int.Parse(parts[0]),
                            LastName = Unescape(parts[1]),
                            FirstName = Unescape(parts[2]),
                            MiddleName = Unescape(parts[3]),
                            Subject = Unescape(parts[4]),
                            Experience = int.Parse(parts[5]),
                            PriceMin = decimal.Parse(parts[6]),
                            PriceMax = decimal.Parse(parts[7]),
                            Education = Unescape(parts[8]),
                            Description = Unescape(parts[9]),
                            Rating = double.Parse(parts[10]),
                            ReviewsCount = int.Parse(parts[11]),
                            CreatedAt = parts.Length > 12 ? DateTime.Parse(parts[12]) : DateTime.Now
                        });
                    }
                }
            }
            return teachers;
        }

       
        public async Task<List<Teacher>> GetAllTeachersAsync(bool includeDeleted = false)
        {
            string command = includeDeleted ? "CMD:GET_ALL_TEACHERS:with_deleted" : "CMD:GET_ALL_TEACHERS";
            string response = await SendAndReceiveAsync(command);

            var teachers = new List<Teacher>();

            if (response?.StartsWith("RESPONSE:TEACHERS:") == true)
            {
                string data = response.Replace("RESPONSE:TEACHERS:", "");
                string[] items = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    string[] parts = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 12)  // вместо 14
                    {
                        teachers.Add(new Teacher
                        {
                            Id = int.Parse(parts[0]),
                            LastName = Unescape(parts[1]),
                            FirstName = Unescape(parts[2]),
                            MiddleName = Unescape(parts[3]),
                            Subject = Unescape(parts[4]),
                            Experience = int.Parse(parts[5]),
                            PriceMin = decimal.Parse(parts[6]),
                            PriceMax = decimal.Parse(parts[7]),
                            Education = Unescape(parts[8]),
                            Description = Unescape(parts[9]),
                            Rating = double.Parse(parts[10]),
                            ReviewsCount = int.Parse(parts[11]),
                            IsDeleted = parts.Length > 12 ? bool.Parse(parts[12]) : false,
                            CreatedAt = parts.Length > 13 ? DateTime.Parse(parts[13]) : DateTime.Now
                        });
                    }
                }
            }
            return teachers;
        }

        
        public async Task<int> AddTeacherAsync(Teacher teacher)
        {
            string command = $"CMD:ADD_TEACHER:{Escape(teacher.LastName)}:{Escape(teacher.FirstName)}:{Escape(teacher.MiddleName ?? "-")}:{Escape(teacher.Subject)}:{teacher.Experience}:{teacher.PriceMin}:{teacher.PriceMax}:{Escape(teacher.Education ?? "-")}:{Escape(teacher.Description ?? "-")}:admin";
            string response = await SendAndReceiveAsync(command);

            if (response?.StartsWith("RESPONSE:ADDED:ID=") == true)
            {
                return int.Parse(response.Replace("RESPONSE:ADDED:ID=", ""));
            }
            return 0;
        }

        
        public async Task<bool> DeleteTeacherAsync(int teacherId)
        {
            string response = await SendAndReceiveAsync($"CMD:DELETE_TEACHER:{teacherId}");
            return response?.StartsWith("RESPONSE:DELETED:") == true;
        }

        
        public async Task<bool> RestoreTeacherAsync(int teacherId)
        {
            string response = await SendAndReceiveAsync($"CMD:RESTORE_TEACHER:{teacherId}");
            return response?.StartsWith("RESPONSE:RESTORED:") == true;
        }

        public async Task<List<Student>> GetStudentsAsync(string subject = null)
        {
            string command = $"CMD:GET_STUDENTS:{subject ?? "-"}";
            string response = await SendAndReceiveAsync(command);

            var students = new List<Student>();

            if (response?.StartsWith("RESPONSE:STUDENTS:") == true)
            {
                string data = response.Replace("RESPONSE:STUDENTS:", "");
                string[] items = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    string[] parts = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 9)
                    {
                        students.Add(new Student
                        {
                            Id = int.Parse(parts[0]),
                            LastName = Unescape(parts[1]),
                            FirstName = Unescape(parts[2]),
                            MiddleName = Unescape(parts[3]),
                            Email = Unescape(parts[4]),
                            Goals = Unescape(parts[5]),
                            PreferredSubjects = Unescape(parts[6]),
                            Rating = double.Parse(parts[7]),
                            ReviewsCount = int.Parse(parts[8]),
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = parts.Length > 9 ? DateTime.Parse(parts[9]) : DateTime.Now
                        });
                    }
                }
            }
            return students;
        }
        public async Task<List<Student>> GetAllStudentsAsync(bool includeDeleted = false)
        {
            string command = includeDeleted ? "CMD:GET_ALL_STUDENTS:with_deleted" : "CMD:GET_ALL_STUDENTS";
            string response = await SendAndReceiveAsync(command);

            var students = new List<Student>();

            if (response?.StartsWith("RESPONSE:STUDENTS:") == true)
            {
                string data = response.Replace("RESPONSE:STUDENTS:", "");
                string[] items = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    string[] parts = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 9)
                    {
                        var student = new Student
                        {
                            Id = int.Parse(parts[0]),
                            LastName = Unescape(parts[1]),
                            FirstName = Unescape(parts[2]),
                            MiddleName = Unescape(parts[3]),
                            Email = Unescape(parts[4]),
                            Goals = Unescape(parts[5]),
                            PreferredSubjects = Unescape(parts[6]),
                            Rating = double.Parse(parts[7]),
                            ReviewsCount = int.Parse(parts[8]),
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.Now
                        };
                        students.Add(student);
                    }
                }
            }
            return students;
        }
        
        public async Task<(bool success, int studentId, string error)> RegisterStudentAsync(
            string lastName, string firstName, string middleName,
            string email, string password, string goals, string subjects)
        {
            string command = $"CMD:REGISTER_STUDENT:{Escape(lastName)}:{Escape(firstName)}:{Escape(middleName ?? "-")}:{email}:{password}:{Escape(goals ?? "-")}:{Escape(subjects ?? "-")}";
            string response = await SendAndReceiveAsync(command);

            if (response?.StartsWith("RESPONSE:REGISTERED:ID=") == true)
            {
                int id = int.Parse(response.Replace("RESPONSE:REGISTERED:ID=", ""));
                return (true, id, null);
            }
            else if (response?.StartsWith("RESPONSE:ERROR:") == true)
            {
                string error = response.Replace("RESPONSE:ERROR:", "");
                return (false, 0, error);
            }
            return (false, 0, "UNKNOWN_ERROR");
        }

        
        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            string response = await SendAndReceiveAsync($"CMD:DELETE_STUDENT:{studentId}");
            return response?.StartsWith("RESPONSE:DELETED:") == true;
        }

        
        public async Task<bool> RestoreStudentAsync(int studentId)
        {
            string response = await SendAndReceiveAsync($"CMD:RESTORE_STUDENT:{studentId}");
            return response?.StartsWith("RESPONSE:RESTORED:") == true;
        }

        
        public async Task<bool> DeactivateStudentAsync(int studentId)
        {
            string response = await SendAndReceiveAsync($"CMD:DEACTIVATE_STUDENT:{studentId}");
            return response?.StartsWith("RESPONSE:DEACTIVATED:") == true;
        }

        
        public async Task<bool> ActivateStudentAsync(int studentId)
        {
            string response = await SendAndReceiveAsync($"CMD:ACTIVATE_STUDENT:{studentId}");
            return response?.StartsWith("RESPONSE:ACTIVATED:") == true;
        }

        public async Task<(bool success, int userId, string role, string error)> LoginAsync(string email, string password)
        {
            string response = await SendAndReceiveAsync($"CMD:LOGIN:{email}:{password}");

            if (response?.StartsWith("RESPONSE:LOGIN_SUCCESS:ID=") == true)
            {
                string data = response.Replace("RESPONSE:LOGIN_SUCCESS:ID=", "");
                string[] parts = data.Split(':');
                if (parts.Length >= 2)
                {
                    int id = int.Parse(parts[0]);
                    string role = parts[1].Replace("ROLE=", "");
                    return (true, id, role, null);
                }
            }
            else if (response?.StartsWith("RESPONSE:ERROR:") == true)
            {
                string error = response.Replace("RESPONSE:ERROR:", "");
                return (false, 0, null, error);
            }
            return (false, 0, null, "UNKNOWN_ERROR");
        }

        
        public async Task<bool> AddReviewAsync(int teacherId, int rating, string comment)
        {
            string command = $"CMD:ADD_REVIEW:{SessionManager.CurrentUserId}:{teacherId}:{rating}:{Escape(comment)}";
            string response = await SendAndReceiveAsync(command);

            return response == "RESPONSE:REVIEW_ADDED";
        }

        
        public async Task<List<Review>> GetAllReviewsAsync(bool showModerated = false)
        {
            string command = showModerated ? "CMD:GET_ALL_REVIEWS:all" : "CMD:GET_ALL_REVIEWS:unmoderated";
            string response = await SendAndReceiveAsync(command);

            var reviews = new List<Review>();

            if (response?.StartsWith("RESPONSE:REVIEWS:") == true)
            {
                string data = response.Replace("RESPONSE:REVIEWS:", "");
                string[] items = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    string[] parts = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 8)
                    {
                        reviews.Add(new Review
                        {
                            Id = int.Parse(parts[0]),
                            FromStudentId = string.IsNullOrEmpty(parts[1]) ? (int?)null : int.Parse(parts[1]),
                            ToTeacherId = string.IsNullOrEmpty(parts[2]) ? (int?)null : int.Parse(parts[2]),
                            FromTeacherId = string.IsNullOrEmpty(parts[3]) ? (int?)null : int.Parse(parts[3]),
                            ToStudentId = string.IsNullOrEmpty(parts[4]) ? (int?)null : int.Parse(parts[4]),
                            Rating = int.Parse(parts[5]),
                            Comment = Unescape(parts[6]),
                            IsModerated = bool.Parse(parts[7]),
                            CreatedAt = DateTime.Parse(parts[8]),
                            FromName = Unescape(parts[9]),
                            ToName = Unescape(parts[10])
                        });
                    }
                }
            }
            return reviews;
        }

        
        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            string response = await SendAndReceiveAsync($"CMD:APPROVE_REVIEW:{reviewId}");
            return response?.StartsWith("RESPONSE:APPROVED:") == true;
        }

        
        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            string response = await SendAndReceiveAsync($"CMD:DELETE_REVIEW:{reviewId}");
            return response?.StartsWith("RESPONSE:DELETED:") == true;
        }

        public async Task<List<Subject>> GetSubjectsAsync()
        {
            string response = await SendAndReceiveAsync("CMD:GET_SUBJECTS");
            var subjects = new List<Subject>();

            if (response?.StartsWith("RESPONSE:SUBJECTS:") == true)
            {
                string data = response.Replace("RESPONSE:SUBJECTS:", "");
                string[] items = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in items)
                {
                    string[] parts = item.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        subjects.Add(new Subject
                        {
                            Id = int.Parse(parts[0]),
                            Name = Unescape(parts[1]),
                            Category = Unescape(parts[2])
                        });
                    }
                }
            }
            return subjects;
        }




    }
}
