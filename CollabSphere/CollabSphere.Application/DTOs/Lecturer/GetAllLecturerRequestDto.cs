using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Lecturer
{
    public class GetAllLecturerRequestDto
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int Yob { get; set; } = 0;
        public string? LecturerCode { get; set; }
        public string? Major { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IsDesc { get; set; } = false;
    }
}
