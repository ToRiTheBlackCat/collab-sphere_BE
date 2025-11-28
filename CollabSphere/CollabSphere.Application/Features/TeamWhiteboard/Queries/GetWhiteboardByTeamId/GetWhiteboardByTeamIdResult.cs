using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardByTeamId
{
    public class GetWhiteboardByTeamIdResult : QueryResult
    {
        public int WhiteboardId { get; set; }
    }
}
