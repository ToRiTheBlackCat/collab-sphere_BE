using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Queries.GetCheckpointDetail
{
    public class GetCheckpointDetailQuery : IQuery<GetCheckpointDetailResult>
    {
        public int CheckpontId { get; set; } = -1;

        public int UserId { get; set; } = -1;

        public int UserRole { get; set; } = -1;
    }
}
