using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.TeamFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamFiles.Queries.GetTeamFiles
{
    public class GetTeamFilesResult : QueryResult
    {
        public Dictionary<string, IGrouping<string, TeamFileVM>> Grouping { get; set; } = new();
    }
}
