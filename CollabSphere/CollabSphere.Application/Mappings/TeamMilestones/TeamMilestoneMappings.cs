using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.MilestoneQuestions;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.TeamMilestones
{
    public static class TeamMilestoneMappings
    {
        public static TeamMilestoneVM ToTeamMilestoneVM(this TeamMilestone entity)
        {
            return new TeamMilestoneVM()
            {
                TeamMilestoneId = entity.TeamMilestoneId,
                ObjectiveMilestoneId = entity.ObjectiveMilestoneId,
                TeamId = entity.TeamId,
                Title = entity.Title,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Progress = entity.Progress,
                Status = entity.Status,
                CheckpointCount = entity.Checkpoints.Count,
                MilestoneQuestionCount = entity.MilestoneQuestions.Count,
            };
        }

        public static List<TeamMilestoneVM> ToTeamMilestoneVM(this IEnumerable<TeamMilestone> entityList)
        {
            if (entityList == null || !entityList.Any())
            {
                return new List<TeamMilestoneVM>();
            }

            return entityList.Select(mile => mile.ToTeamMilestoneVM()).ToList();
        }
    }
}
