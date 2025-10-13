using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Lecturer
{
    public class GetAllLecturerResponseDto
    {
        public int ItemCount { get; set; }
        public int PageNum { get; set; }

        public int PageSize { get; set; }

        public int PageCount => (int)Math.Ceiling(ItemCount * 1.0 / PageSize);

        public List<LecturerResponseDto> LecturerList { get; set; } = new List<LecturerResponseDto>();
    }

    public class LecturerResponseDto
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

        public string LecturerCode { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }
}
