using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Teams
{
    public class TeamDetailDto
    {
        #region Team Info
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamImage { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public string EnrolKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GitLink { get; set; } = string.Empty;
        public DateOnly CreatedDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int Status { get; set; }
        #endregion

        #region Other Info
        public TeamProgress TeamProgress { get; set; } = new TeamProgress();

        public LecturerInfo LecturerInfo { get; set; } = new LecturerInfo();

        public ClassInfo ClassInfo { get; set; } = new ClassInfo();

        public ProjectInfo ProjectInfo { get; set; } = new ProjectInfo();

        public MemberInfo MemberInfo { get; set; } = new MemberInfo();
        #endregion
    }

    public class TeamProgress
    {
        public float? OverallProgress { get; set; } = 0;
        public float? MilestonesProgress { get; set; } = 0;
        public int? TotalMilestones { get; set; } = 0;
        public int? MilestonesComplete { get; set; } = 0;
        public float? CheckPointProgress { get; set; } = 0;
        public int? TotalCheckpoints { get; set; } = 0;
        public int? CheckpointsComplete { get; set; } = 0;
    }
    public class ClassInfo
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }
    public class ProjectInfo
    {
        public int? ProjectAssignmentId { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectBusinessRule { get; set; } = string.Empty;
        public string ProjectActors { get; set; } = string.Empty;


    }
    public class LecturerInfo
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
    }
    public class MemberInfo
    {
        public int MemberCount { get; set; }
        public List<TeamMemberInfo> Members { get; set; } = new List<TeamMemberInfo>();
    }
    public class TeamMemberInfo
    {
        public int ClassMemberId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public int? TeamRole { get; set; }
        public float? CheckpointContributionPercentage { get; set; } = 0;
        public float? MilestoneAnsContributionPercentage { get; set; } = 0;

    }
}
