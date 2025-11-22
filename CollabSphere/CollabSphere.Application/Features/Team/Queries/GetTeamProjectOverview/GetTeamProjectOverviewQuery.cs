using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetTeamProjectOverview
{
    public class GetTeamProjectOverviewQuery : IQuery<GetTeamProjectOverviewResult>
    {
        public int TeamId { get; set; }

        public int UserId = -1;

        public int UserRole = -1;
    }
}
