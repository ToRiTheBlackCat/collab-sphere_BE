using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Student
{
    public class StudentOfClassDto
    {
        #region Student Info

        public int UId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Fullname { get; set; } = string.Empty;

        public string AvatarPublicId { get; set; } = string.Empty;

        public string StudentCode { get; set; } = string.Empty;

        public string Major { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        #endregion

        public DetailInfo DetailInfo { get; set; }
        public TeamInfo TeamInfo { get; set; }
        public ProjectInfoOfStudent ProjectInfo { get; set; }

    }
    public class DetailInfo
    {
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = "0";
        public int? Yob { get; set; }
        public string School { get; set; } = string.Empty;
    }

    public class TeamInfo
    {
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? TeamRole { get; set; } 
    }
    public class ProjectInfoOfStudent
    {
        public int? ProjectAssignmentId { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; } 
    }
}
