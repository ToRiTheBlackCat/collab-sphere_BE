using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Semesters
{
    public class SemesterVM
    {
        public int SemesterId { get; set; }

        public string SemesterName { get; set; }

        public string SemesterCode { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }
    }
}
