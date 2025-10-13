using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.User
{
    public class UserProfileDto
    {
        public int UId { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public string Fullname { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public int? Yob { get; set; }

        public string AvatarImg { get; set; }

        public string School { get; set; }

        public bool IsTeacher { get; set; }

        public string Code { get; set; }

        public string Major { get; set; }

    }
}
