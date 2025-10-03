using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.User
{
    public class AdminGetAllUsersResponseDto
    {
        public int HeadDepartmentCount { get; set; }
        public int StaffCount { get; set; }
        public int LecturerCount { get; set; }
        public int StudentCount { get; set; }

        public List<Admin_AllHeadDepartment_StaffDto> HeadDepartmentList { get; set; } = new List<Admin_AllHeadDepartment_StaffDto>();
        public List<Admin_AllHeadDepartment_StaffDto> StaffList { get; set; } = new List<Admin_AllHeadDepartment_StaffDto>();
        public List<Admin_AllLecturerDto> LecturerList { get; set; } = new List<Admin_AllLecturerDto>();
        public List<Admin_AllStudentDto> StudentList { get; set; } = new List<Admin_AllStudentDto>();

    }
    public class Admin_AllHeadDepartment_StaffDto
    {
        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public bool IsTeacher { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }

    public class Admin_AllLecturerDto
    {
        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public bool IsTeacher { get; set; }

        public string Fullname { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int? Yob { get; set; }

        public string AvatarPublicId { get; set; } = string.Empty;

        public string School { get; set; } = string.Empty;

        public string LecturerCode { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }

    public class Admin_AllStudentDto
    {
        public string Email { get; set; } = string.Empty;

        public string RoleName { get; set; } = string.Empty;

        public bool IsTeacher { get; set; }

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
