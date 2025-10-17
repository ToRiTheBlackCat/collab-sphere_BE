using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Queries.GetStudentOfClass
{
    public class GetStudentOfClassResult : QueryResult
    {
        public PagedList<StudentOfClassDto>? StudentsOfClass {  get; set; }
    }
}
