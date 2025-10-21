using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.CreateCheckpoint
{
    public class CreateCheckpointResult : CommandResult
    {
        public int CheckpointId { get; set; }
    }
}
