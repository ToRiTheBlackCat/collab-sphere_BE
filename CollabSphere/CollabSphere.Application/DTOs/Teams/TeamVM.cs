using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Teams
{
    public class TeamVM
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public string TeamImage { get; set; }

        public string Description { get; set; }

        public string GitLink { get; set; }

        public int ClassId { get; set; }

        #region Team Project Info
        public int? ProjectAssignmentId { get; set; }

        public int? ProjectId { get; set; }

        public string ProjectName { get; set; } 
        #endregion

        public DateOnly CreatedDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public float? Progress { get; set; }

        public int Status { get; set; }

        public static explicit operator TeamVM(Domain.Entities.Team team)
        {
            var projectAssign = team.ProjectAssignment;

            return new TeamVM()
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                TeamImage = team.TeamImage,
                Description = team.Description,
                GitLink = team.GitLink,
                ClassId = team.ClassId,
                ProjectAssignmentId = team.ProjectAssignmentId,
                ProjectId = projectAssign != null ? projectAssign.Project.ProjectId : null,
                ProjectName = projectAssign != null ? projectAssign.Project.ProjectName : string.Empty,
                CreatedDate = team.CreatedDate,
                EndDate = team.EndDate,
                Progress = team.Progress,
                Status = team.Status,
            };
        }
    }
}
