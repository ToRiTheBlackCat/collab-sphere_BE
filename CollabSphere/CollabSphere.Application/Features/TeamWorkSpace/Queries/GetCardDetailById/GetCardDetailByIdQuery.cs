using CollabSphere.Application.Base;
using CollabSphere.Application.Features.TeamWorkSpace.Queries.GetTeamWorkspaceByTeam;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Queries.GetCardDetailById
{
    public class GetCardDetailByIdQuery : IQuery<GetCardDetailByIdResult>
    {
        [FromRoute(Name = "cardId")]
        public int CardId { get; set; }
        [Required]
        public int WorkspaceId { get; set; }
        [Required]
        public int ListId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
