using CollabSphere.Application.Base;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetStudentClasses
{
    public class GetStudentClassesQuery : PaginationQuery, IQuery<GetStudentClassesResult>
    {
        [FromRoute(Name = "studentId")]
        public int StudentId { get; set; }

        [FromQuery]
        public string Descriptor { get; set; } = string.Empty;

        [FromQuery]
        public int? SemesterId { get; set; }

        [FromQuery]
        public HashSet<int>? SubjectIds { get; set; }

        [FromQuery]
        public HashSet<int>? LecturerIds { get; set; }

        [FromQuery]
        public bool? IsActive { get; set; }

        [FromQuery]
        public string OrderBy { get; set; } = string.Empty;

        [FromQuery]
        public bool Descending { get; set; } = false;
    }
}
