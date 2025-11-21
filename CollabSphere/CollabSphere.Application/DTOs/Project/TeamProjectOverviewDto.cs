using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Mappings.Objectives;
using CollabSphere.Application.Mappings.TeamMilestones;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Project
{
    public class TeamProjectOverviewDto
    {
        public required int ProjectId { get; set; }

        public required string ProjectName { get; set; } = string.Empty;

        public required string Description { get; set; } = string.Empty;

        #region Lecturer Info
        public required int LecturerId { get; set; }

        public required string LecturerName { get; set; }

        public required string LecturerCode { get; set; }
        #endregion

        public required int TeamId { get; set; }

        public List<TeamProjectObjectiveVM> Objectives { get; set; } = new List<TeamProjectObjectiveVM>();

        public List<TeamProjectMilestoneVM> CustomTeamMilestones { get; set; } = new List<TeamProjectMilestoneVM>();
    }
}

namespace CollabSphere.Application.Mappings.Projects
{
    public static partial class ProjectMappings
    {
        public static TeamProjectOverviewDto ExtractProjectInfo(this CollabSphere.Domain.Entities.Team team)
        {
            if (team.ProjectAssignmentId == null)
            {
                throw new Exception($"Team '{team.TeamName}'({team.TeamId}) doesn't have an assigned project.");
            }
            var project = team.ProjectAssignment.Project;
            var objectives = project.Objectives;

            return new TeamProjectOverviewDto()
            {
                ProjectId = team.ProjectAssignment.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                LecturerId = project.LecturerId,
                LecturerName = project.Lecturer.Fullname,
                LecturerCode = project.Lecturer.LecturerCode,
                TeamId = team.TeamId,
                Objectives = project.Objectives.ToTeamProjectobjectiveVM(),
                CustomTeamMilestones = team.TeamMilestones.ToTeamProjectMilestoneVMs()
            };
        }
    }
}
