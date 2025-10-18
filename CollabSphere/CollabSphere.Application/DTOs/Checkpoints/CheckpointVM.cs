using CollabSphere.Application.Constants;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Checkpoints
{
    public class CheckpointVM
    {
        public int CheckpointId { get; set; }

        public int TeamMilestoneId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Complexity { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly DueDate { get; set; }

        public int Status { get; set; }

        public string StatusString => ((CheckpointStatuses)this.Status).ToString();

        public int FileCount { get; set; }

        public List<CheckpointAssignmentVM> CheckpointAssignments { get; set; } = new List<CheckpointAssignmentVM>();

        //public virtual ICollection<CheckpointFile> CheckpointFiles { get; set; } = new List<CheckpointFile>();

        //public virtual ICollection<CheckpointSubmit> CheckpointSubmits { get; set; } = new List<CheckpointSubmit>();

        public static explicit operator CheckpointVM(Checkpoint checkpoint)
        {
            return new CheckpointVM()
            {
                 CheckpointId = checkpoint.CheckpointId,
                 TeamMilestoneId = checkpoint.TeamMilestoneId,
                 Title = checkpoint.Title,
                 Description = checkpoint.Description,
                 Complexity = checkpoint.Complexity,
                 StartDate = checkpoint.StartDate!.Value,
                 DueDate = checkpoint.DueDate,
                 Status = checkpoint.Status,
                 FileCount = checkpoint.CheckpointFiles.Count,
                 CheckpointAssignments = checkpoint.CheckpointAssignments.Select(assign => (CheckpointAssignmentVM)assign).ToList()
            };
        }
    }
}
