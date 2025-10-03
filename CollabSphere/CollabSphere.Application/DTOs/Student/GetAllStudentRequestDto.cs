using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Student
{
    public class GetAllStudentRequestDto
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int Yob { get; set; }
        public string? StudentCode { get; set; }
        public string? Major { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
