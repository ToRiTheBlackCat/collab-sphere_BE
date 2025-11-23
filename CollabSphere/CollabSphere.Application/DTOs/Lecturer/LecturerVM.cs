using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Lecturer
{
    public class LecturerVM
    {
        public int LecturerId { get; set; }

        public string LecturerName { get; set; } = string.Empty;

        public string LecturerCode { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int? Yob { get; set; }

        public string AvatarImg { get; set; } = string.Empty;

        public string School { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.Lecturer
{
    public static partial class LecturerMappings
    {
        public static LecturerVM ToViewModel(this CollabSphere.Domain.Entities.Lecturer lecturer)
        {
            return new LecturerVM()
            {
                LecturerId = lecturer.LecturerId,
                LecturerName = lecturer.Fullname,
                LecturerCode = lecturer.LecturerCode,
                Email = lecturer.LecturerNavigation.Email,
                Address = lecturer.Address,
                PhoneNumber = lecturer.PhoneNumber,
                Yob = lecturer.Yob,
                AvatarImg = lecturer.AvatarImg,
                School = lecturer.School,
                Major = lecturer.Major,
                IsActive = lecturer.LecturerNavigation.IsActive,
            };
        }
    }
}
