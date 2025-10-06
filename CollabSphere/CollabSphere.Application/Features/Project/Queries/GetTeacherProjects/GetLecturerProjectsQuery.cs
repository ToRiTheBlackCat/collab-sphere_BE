using CollabSphere.Application.Base;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetTeacherProjects
{
    public class GetLecturerProjectsQuery : IQuery<GetLecturerProjectResult>
    {
        [FromRoute(Name = "lecturerId")]
        public int LecturerId { get; set; }

        [FromQuery]
        public HashSet<int> Statuses { get; set; } = new HashSet<int>();
    }
}
