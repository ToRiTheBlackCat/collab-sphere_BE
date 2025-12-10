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

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class TeamMilestoneVM
    {
        public int TeamMilestoneId { get; set; }

        public int? ObjectiveMilestoneId { get; set; }

        public int TeamId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;
        
        public DateOnly? StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public float? Progress { get; set; }

        public int Status { get; set; }

        public string StatusString => ((TeamMilestoneStatuses)this.Status).ToString();

        public int CheckpointCount { get; set; }

        public int MilestoneQuestionCount { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.TeamMilestones
{
    public static partial class TeamMilestoneMappings
    {
        public static TeamMilestoneVM ToTeamMilestoneVM(this TeamMilestone entity)
        {
            return new TeamMilestoneVM()
            {
                TeamMilestoneId = entity.TeamMilestoneId,
                ObjectiveMilestoneId = entity.SyllabusMilestoneId,
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
