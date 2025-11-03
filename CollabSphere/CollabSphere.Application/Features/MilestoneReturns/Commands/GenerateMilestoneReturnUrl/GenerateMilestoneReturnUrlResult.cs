using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.MilestoneReturns.Commands.GenerateMilestoneReturnUrl
{
    public class GenerateMilestoneReturnUrlResult : CommandResult
    {
        public string FileUrl { get; set; } = null!;

        public DateTime UrlExpireTime { get; set; }
    }
}
