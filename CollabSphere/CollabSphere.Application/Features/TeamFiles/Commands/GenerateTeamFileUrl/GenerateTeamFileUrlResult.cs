using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Commands.GenerateTeamFileUrl
{
    public class GenerateTeamFileUrlResult : CommandResult
    {
        public string FileUrl { get; set; } = null!;

        public DateTime UrlExpireTime { get; set; }
    }
}
