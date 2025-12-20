using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.LecturerGetAllEvaluationsInTeam
{
    public class LecturerGetAllEvaluationsInTeamQuery : IQuery<LecturerGetAllEvaluationsInTeamResult>
    {
        [FromRoute(Name = "teamId")]
        public int TeamId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
