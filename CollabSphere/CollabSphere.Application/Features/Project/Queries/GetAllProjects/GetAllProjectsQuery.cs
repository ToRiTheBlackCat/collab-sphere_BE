using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetAllProjects
{
    public class GetAllProjectsQuery : PaginationQuery, IQuery<GetAllProjectsResult>
    {
        [FromQuery]
        public List<int> LecturerIds { get; set; } = new List<int>();

        [FromQuery]
        public List<int> SubjectIds { get; set; } = new List<int>();

        [FromQuery]
        public string Descriptors { get; set; } = string.Empty;
    }
}
