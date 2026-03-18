using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTutor.Services
{
    public static class SessionManager
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUserRole { get; set; }
        public static bool IsLoggedIn => CurrentUserId > 0;
        public static bool IsAdmin => CurrentUserRole == "Admin";
        public static bool IsStudent => CurrentUserRole == "Student";

        public static void Login(int userId, string role)
        {
            CurrentUserId = userId;
            CurrentUserRole = role;
        }

        public static void Logout()
        {
            CurrentUserId = 0;
            CurrentUserRole = null;
        }
    }
}
