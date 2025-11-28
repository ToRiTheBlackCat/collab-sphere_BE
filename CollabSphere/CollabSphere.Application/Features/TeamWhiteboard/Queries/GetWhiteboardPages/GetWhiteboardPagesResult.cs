using CollabSphere.Application.Base;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardPages
{
    public class GetWhiteboardPagesResult : QueryResult
    {
        public List<WhiteboardPage>? Pages { get; set; } = new List<WhiteboardPage>();
    }
}
