using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.MilestoneFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneFiles.Commands.GenerateMilestoneFileUrl
{
    public class GenerateMilestoneFileUrlResult : CommandResult
    {
        public string FileUrl { get; set; } = null!;

        public DateTime UrlExpireTime { get; set; }
    }
}
