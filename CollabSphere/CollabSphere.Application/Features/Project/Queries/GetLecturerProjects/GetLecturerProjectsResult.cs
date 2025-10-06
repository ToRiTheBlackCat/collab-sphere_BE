using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetTeacherProjects
{
    public class GetLecturerProjectsResult : QueryResult
    {
        public List<ProjectVM> Projects { get; set; } = new List<ProjectVM>();
    }
}
