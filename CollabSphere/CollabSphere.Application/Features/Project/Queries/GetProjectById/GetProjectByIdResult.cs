using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetProjectById
{
    public class GetProjectByIdResult : QueryResult
    {
        public ProjectVM? Project { get; set; }
        public bool IsAuthorized { get; set; } = true;
    }
}
