using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetProjectById
{
    public class GetProjectByIdQuery : IQuery<GetProjectByIdResult>
    {
        [FromRoute(Name = "projectId")]
        public int ProjectId { get; set; }

        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;
    }
}
