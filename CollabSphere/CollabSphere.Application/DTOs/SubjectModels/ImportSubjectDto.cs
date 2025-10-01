using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.SubjectModels
{
    public class ImportSubjectDto
    {
        public string SubjectName { get; set; }

        public string SubjectCode { get; set; }

        public bool IsActive { get; set; }
    }
}
