using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class TeamProjectMilestoneVM
    {
        public required int TeamMilestoneId { get; set; }

        #region Objective Info
        public int? ObjectiveId { get; set; }
        #endregion

        public required int TeamId { get; set; }

        public required string Title { get; set; } = null!;

        public required string Description { get; set; } = null!;

        public required DateOnly? StartDate { get; set; }

        public required DateOnly EndDate { get; set; }

        public required float? Progress { get; set; }

        public required string Status { get; set; } = null!;
    }
}

namespace CollabSphere.Application.Mappings.TeamMilestones
{
    public static partial class ObjectiveMappings
    {
        public static TeamProjectMilestoneVM ToTeamProjectMilestoneVM(this TeamMilestone teamMilestone)
        {
            int? objectiveId = null;
            if (teamMilestone.ObjectiveMilestoneId.HasValue)
            {
                objectiveId = teamMilestone.ObjectiveMilestone.ObjectiveId;
            }

            return new TeamProjectMilestoneVM()
            {
                TeamMilestoneId = teamMilestone.TeamMilestoneId,
                TeamId = teamMilestone.TeamId,
                ObjectiveId = objectiveId,
                Title = teamMilestone.Title,
                Description = teamMilestone.Description,
                StartDate = teamMilestone.StartDate,
                EndDate = teamMilestone.EndDate,
                Progress = teamMilestone.Progress,
                Status = ((TeamMilestoneStatuses)teamMilestone.Status).ToString()
            };
        }

        public static List<TeamProjectMilestoneVM> ToTeamProjectMilestoneVMs(this IEnumerable<TeamMilestone> teamMilestones)
        {
            if (teamMilestones == null || !teamMilestones.Any())
            {
                return new List<TeamProjectMilestoneVM>();
            }

            return teamMilestones.Select(ojt => ojt.ToTeamProjectMilestoneVM()).ToList();
        }
    }
}
