using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchTutor.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Goals { get; set; }
        public string PreferredSubjects { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        public string RatingDisplay => $"{Rating:F1} ★ ({ReviewsCount})";
    }
}
