using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestonesByTeam
{
    public class GetMilestonesByTeamQuery : IQuery<GetMilestonesByTeamResult>
    {
        public int TeamId  = -1;

        public int UserId  = -1;

        public int UserRole  = -1;
    }
}
