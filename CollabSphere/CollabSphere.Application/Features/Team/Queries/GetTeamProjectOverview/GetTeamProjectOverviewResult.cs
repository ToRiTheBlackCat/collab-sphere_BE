using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetTeamProjectOverview
{
    public class GetTeamProjectOverviewResult : QueryResult
    {
        public TeamProjectOverviewDto ProjectOverview { get; set; } = null!;
    }
}
