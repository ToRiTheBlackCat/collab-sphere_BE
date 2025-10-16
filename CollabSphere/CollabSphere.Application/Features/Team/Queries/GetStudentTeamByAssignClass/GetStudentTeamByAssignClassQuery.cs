﻿using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetStudentTeamByAssignClass
{
    public class GetStudentTeamByAssignClassQuery : IQuery<GetStudentTeamByAssignClassResult>
    {
        [FromRoute(Name = "classId")]
        public int ClassId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
