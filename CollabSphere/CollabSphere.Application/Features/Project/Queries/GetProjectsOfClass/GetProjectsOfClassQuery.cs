using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetProjectsOfClass
{
    public class GetProjectsOfClassQuery : IQuery<GetProjectsOfClassResult>
    {
        [FromRoute(Name = "ClassId")]
        public int ClassId { get; set; }

        public string Descriptors { get; set; } = string.Empty;

        public int PageNum { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
