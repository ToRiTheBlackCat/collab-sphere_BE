using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.MilestoneQuestions;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.TeamMilestones
{
    public class TeamMilestoneDetailDto
    {
        public int TeamMilestoneId { get; set; }

        public int ObjectiveMilestoneId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int TeamId { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public float? Progress { get; set; }

        public int Status { get; set; }

        public virtual ICollection<CheckpointVM> Checkpoints { get; set; } = new List<CheckpointVM>();

        public virtual ICollection<TeamMilestoneQuestionDto> MilestoneQuestions { get; set; } = new List<TeamMilestoneQuestionDto>();

        public static explicit operator TeamMilestoneDetailDto(TeamMilestone teamMilestone)
        {
            return new TeamMilestoneDetailDto()
            {
                TeamMilestoneId = teamMilestone.TeamMilestoneId,
                ObjectiveMilestoneId = teamMilestone.ObjectiveMilestoneId,
                Title = teamMilestone.Title,
                Description = teamMilestone.Description,
                TeamId = teamMilestone.TeamId,
                StartDate = teamMilestone.StartDate,
                EndDate = teamMilestone.EndDate,
                Progress = teamMilestone.Progress,
                Status = teamMilestone.Status,
                Checkpoints = teamMilestone.Checkpoints.Select(x => (CheckpointVM)x).ToList(),
                MilestoneQuestions = teamMilestone.MilestoneQuestions.Select(x => (TeamMilestoneQuestionDto)x).ToList(),
            };
        }
    }
}
