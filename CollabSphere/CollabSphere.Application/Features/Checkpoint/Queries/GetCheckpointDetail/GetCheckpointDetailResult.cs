using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Checkpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoint.Queries.GetCheckpointDetail
{
    public class GetCheckpointDetailResult : QueryResult
    {
        public CheckpointDetailDto? Checkpoint { get; set; }
    }
}
