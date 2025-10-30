using CollabSphere.Application.Constants;
using CollabSphere.Application.Mappings;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Checkpoints
{
    public class CheckpointDetailDto
    {
        public int CheckpointId { get; set; }

        public int TeamMilestoneId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Complexity { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly DueDate { get; set; }

        /// <summary>
        /// 0 - not done , 1 - done
        /// </summary>
        public int Status { get; set; }

        public string StatusString => ((CheckpointStatuses)this.Status).ToString();

        public List<CheckpointAssignmentVM> CheckpointAssignments { get; set; } = new List<CheckpointAssignmentVM>();

        public List<CheckpointFileVM> CheckpointFiles { get; set; } = new List<CheckpointFileVM>();

        public static explicit operator CheckpointDetailDto(Checkpoint checkpoint)
        {
            return new CheckpointDetailDto()
            {
                CheckpointId = checkpoint.CheckpointId,
                TeamMilestoneId = checkpoint.TeamMilestoneId,
                Title = checkpoint.Title,
                Description = checkpoint.Description,
                Complexity = checkpoint.Complexity,
                StartDate = checkpoint.StartDate,
                DueDate = checkpoint.DueDate,
                Status = checkpoint.Status,
                CheckpointAssignments = checkpoint.CheckpointAssignments.Select(x => (CheckpointAssignmentVM)x).ToList(),
                CheckpointFiles = checkpoint.CheckpointFiles.ToViewModel(),
            };
        }
    }
}
