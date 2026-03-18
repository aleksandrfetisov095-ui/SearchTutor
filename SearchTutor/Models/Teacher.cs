using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchTutor.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Subject { get; set; }
        public int Experience { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceMax { get; set; }
        public string Education { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public string AddedByAdmin { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        public string PriceRange => $"{PriceMin} - {PriceMax} руб.";
        public string RatingDisplay => $"{Rating:F1} ★ ({ReviewsCount})";
    }
}
