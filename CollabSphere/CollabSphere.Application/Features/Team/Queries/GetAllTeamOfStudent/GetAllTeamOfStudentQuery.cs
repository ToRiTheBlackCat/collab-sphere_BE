using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetAllTeamOfStudent
{
    public class GetAllTeamOfStudentQuery : PaginationQuery, IQuery<GetAllTeamOfStudentResult>
    {
        [FromRoute(Name = "studentId")]
        public int StudentId { get; set; }

        [JsonIgnore]
        public int ViewerUId;

        [JsonIgnore]
        public int ViewerRole;

        [FromQuery]
        public string? TeamName { get; set; }

        [FromQuery]
        public int? ClassId { get; set; }
    }
}
