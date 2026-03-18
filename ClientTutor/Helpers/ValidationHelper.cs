using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClientTutor.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmailSimple(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();
         
            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex != email.LastIndexOf('@'))
                return false;
      
            int dotIndex = email.IndexOf('.', atIndex + 1);
            if (dotIndex <= atIndex + 1)
                return false;

            if (email.Contains(" "))
                return false;

            return true;
        }

        public static string GetEmailErrorMessage()
        {
            return "Введите корректный email (например: user@mail.com)";
        }
    }
}
