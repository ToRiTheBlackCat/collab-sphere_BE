using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Classes.Queries.GetAllClasses;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Classes.Queries.GetLecturerClasses
{
    public class GetLecturerClassesQuery : PaginationQuery, IQuery<GetLecturerClassesResult>
    {
        [FromRoute(Name = "lecturerId")]
        public int LecturerId { get; set; }

        [FromQuery]
        public string ClassName { get; set; } = string.Empty;

        [FromQuery]
        public int? SemesterId { get; set; }

        [FromQuery]
        public HashSet<int>? SubjectIds { get; set; }

        [FromQuery]
        public string OrderBy { get; set; } = nameof(Class.ClassName);

        [FromQuery]
        public bool Descending { get; set; } = false;
    }
}
