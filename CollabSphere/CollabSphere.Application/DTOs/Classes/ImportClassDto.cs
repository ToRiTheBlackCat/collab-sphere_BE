using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Classes
{
    public class ImportClassDto
    {
        public string ClassName { get; set; }
        public string SubjectCode { get; set; }
        public string SemesterCode { get; set; }
        public string LecturerCode { get; set; }
        public List<string> StudentCodes { get; set; }
        public bool IsActive { get; set; }
    }
}
