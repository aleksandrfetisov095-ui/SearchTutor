using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchTutor.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int? FromStudentId { get; set; }
        public int? ToTeacherId { get; set; }
        public int? FromTeacherId { get; set; }
        public int? ToStudentId { get; set; }
        public int Rating { get; set; } 
        public string Comment { get; set; }
        public bool IsModerated { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
