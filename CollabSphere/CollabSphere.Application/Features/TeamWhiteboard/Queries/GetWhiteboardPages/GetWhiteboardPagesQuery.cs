using CollabSphere.Application.Base;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardByTeamId;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardPages
{
    public class GetWhiteboardPagesQuery : IQuery<GetWhiteboardPagesResult>
    {
        [FromRoute(Name = "whiteboardId")]
        public int WhiteboardId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
