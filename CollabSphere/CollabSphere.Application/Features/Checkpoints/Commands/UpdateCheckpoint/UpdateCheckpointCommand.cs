using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Checkpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.UpdateCheckpoint
{
    public class UpdateCheckpointCommand : ICommand
    {
        public UpdateCheckpointDto UpdateDto { get; set; } = null!;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
