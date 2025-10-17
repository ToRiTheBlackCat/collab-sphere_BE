using CollabSphere.Application.Base;
using CollabSphere.Application.Features.Lecturer.Queries.GetAllLec;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Queries
{
    public class GetAllStudentQuery : PaginationQuery, IQuery<GetAllStudentResult>
    {
        [JsonIgnore]
        public int UserId = -1;

        [JsonIgnore]
        public int UserRole = -1;

        [FromQuery]
        public string? Email { get; set; }

        [FromQuery]
        public string? FullName { get; set; }

        [FromQuery]
        public int Yob { get; set; } = 0;

        [FromQuery]
        public string? StudentCode { get; set; }

        [FromQuery]
        public string? Major { get; set; }

        [FromQuery]
        public bool IsDesc { get; set; } = false;
    }
}
