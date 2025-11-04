using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.DTOs.MilestoneEvaluations;
using CollabSphere.Application.DTOs.MilestoneFiles;
using CollabSphere.Application.DTOs.MilestoneQuestions;
using CollabSphere.Application.DTOs.MilestoneReturns;
using CollabSphere.Application.DTOs.TeamMilestones;
using CollabSphere.Application.Mappings.MilestoneFiles;
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

        public int? ObjectiveMilestoneId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int TeamId { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public float? Progress { get; set; }

        public int Status { get; set; }

        public List<CheckpointVM> Checkpoints { get; set; } = new List<CheckpointVM>();

        public List<TeamMilestoneQuestionVM> MilestoneQuestions { get; set; } = new List<TeamMilestoneQuestionVM>();

        public List<TeamMilestoneFileVM> MilestoneFiles { get; set; } = new List<TeamMilestoneFileVM>();

        public List<TeamMilestoneReturnVM> MilestoneReturns { get; set; } = new List<TeamMilestoneReturnVM>();

        public TeamMilestoneEvaluationVM? MilestoneEvaluation { get; set; }
    }
}

namespace CollabSphere.Application.Mappings.TeamMilestones
{
    public static partial class TeamMilestoneMappings
    {
        public static TeamMilestoneDetailDto ToDetailDto(this TeamMilestone teamMilestone)
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
                Checkpoints = teamMilestone.Checkpoints.Select(cpoint => (CheckpointVM)cpoint).ToList(),
                MilestoneQuestions = teamMilestone.MilestoneQuestions.Select(mQuest => (TeamMilestoneQuestionVM)mQuest).ToList(),
                MilestoneFiles = teamMilestone.MilestoneFiles.ToViewModel(),
                MilestoneReturns = teamMilestone.MilestoneReturns.Select(mReturn => (TeamMilestoneReturnVM)mReturn).ToList(),
                MilestoneEvaluation = teamMilestone.MilestoneEvaluation != null ? (TeamMilestoneEvaluationVM)teamMilestone.MilestoneEvaluation : null,
            };
        }
    }
}
