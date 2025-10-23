using CollabSphere.Application.DTOs.Semesters;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Mappings.Semesters
{
    public static class SemesterMappings
    {
        public static SemesterVM ToSemesterVM(this Semester semester)
        {
            return new SemesterVM()
            {
                SemesterId = semester.SemesterId,
                SemesterName = semester.SemesterName,
                StartDate = semester.StartDate,
                EndDate = semester.EndDate,
            };
        }

        public static List<SemesterVM> ToSemesterVM(this IEnumerable<Semester> semesters)
        {
            return semesters.Select(sem => sem.ToSemesterVM()).ToList();
        }
    }
}
