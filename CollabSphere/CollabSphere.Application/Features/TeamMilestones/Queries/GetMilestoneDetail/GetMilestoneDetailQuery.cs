using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail
{
    public class GetMilestoneDetailQuery : IQuery<GetMilestoneDetailResult>
    {
        public int TeamMilestoneId { get; set; }

        public int UserId = -1;

        public int UserRole = -1;
    }
}
