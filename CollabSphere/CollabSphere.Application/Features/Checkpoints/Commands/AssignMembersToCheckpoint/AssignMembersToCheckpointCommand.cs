using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.CheckpointAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.AssignMembersToCheckpoint
{
    public class AssignMembersToCheckpointCommand : ICommand
    {
        public CheckpointAssignmentsDto AssignmentsDto { get; set; }

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1; 
    }
}
