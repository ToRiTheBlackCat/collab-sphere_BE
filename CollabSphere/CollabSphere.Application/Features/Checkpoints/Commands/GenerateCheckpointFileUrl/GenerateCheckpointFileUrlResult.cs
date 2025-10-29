using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Checkpoints.Commands.GenerateCheckpointFileUrl
{
    public class GenerateCheckpointFileUrlResult : CommandResult
    {
        public string FileUrl { get; set; }

        public DateTime UrlExpireTime { get; set; }
    }
}
