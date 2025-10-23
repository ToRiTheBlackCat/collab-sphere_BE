using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CheckDoneCheckpoint
{
    public class CheckDoneCheckpointCommand : ICommand
    {
        [FromRoute(Name = "checkpointId")]
        public int CheckpointId { get; set; } = -1;

        [FromQuery]
        public bool IsDone { get; set; } = true;

        public int UserId = -1;

        public int UserRole = -1;
    }
}
