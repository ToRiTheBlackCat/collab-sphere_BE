using CollabSphere.Application.DTOs.Lecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Student
{
    public class StudentResponseDto
    {
        public int UId { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool IsTeacher { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string Fullname { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int? Yob { get; set; }

        public string AvatarPublicId { get; set; } = string.Empty;

        public string School { get; set; } = string.Empty;

        public string StudentCode { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }
}
