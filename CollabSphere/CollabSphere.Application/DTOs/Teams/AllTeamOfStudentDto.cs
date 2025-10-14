using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Teams
{
    public class AllTeamOfStudentDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamImage { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public float? Progress { get; set; }
    }
}
