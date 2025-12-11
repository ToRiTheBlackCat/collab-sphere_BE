using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Application.DTOs.SyllabusMilestones;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Mappings.SubjectOutcomes;
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

        public List<TeamProjectSubjectOutcome> SubjectOutcomes { get; set; } = new List<TeamProjectSubjectOutcome>();

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

            var customeTeamMilestones = team.TeamMilestones.Where(x => !x.SyllabusMilestoneId.HasValue).ToList();

            if (project.Subject.SubjectSyllabi.Count != 1)
            {
                throw new Exception("Casting excepton. Critical data error in Database.");
            }

            // Only get team milestone mappings of current team
            var outcomes = project.Subject.SubjectSyllabi.First().SubjectOutcomes;
            foreach (var syllabusMilestone in outcomes.SelectMany(x => x.SyllabusMilestones))
            {
                syllabusMilestone.TeamMilestones = syllabusMilestone.TeamMilestones.Where(x => x.TeamId == team.TeamId).ToList();
            }

            return new TeamProjectOverviewDto()
            {
                ProjectId = team.ProjectAssignment.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                LecturerId = project.LecturerId,
                LecturerName = project.Lecturer.Fullname,
                LecturerCode = project.Lecturer.LecturerCode,
                TeamId = team.TeamId,
                SubjectOutcomes = outcomes.TeamProjectSubjectOutcomes(),
                CustomTeamMilestones = customeTeamMilestones.ToTeamProjectMilestoneVMs()
            };
        }
    }
}
