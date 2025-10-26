using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Semesters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Semesters.Queries.GetAllSemester
{
    public class GetAllSemestersResult : QueryResult
    {
        public List<SemesterVM> Semesters { get; set; } = new List<SemesterVM>();
    }
}
