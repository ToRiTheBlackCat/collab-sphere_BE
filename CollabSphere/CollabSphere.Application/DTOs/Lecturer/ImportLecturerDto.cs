using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Lecturer
{
    public class ImportLecturerDto
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Yob { get; set; }
        public string? School { get; set; }
        public string? LecturerCode { get; set; }
        public string? Major { get; set; }
    }
}
