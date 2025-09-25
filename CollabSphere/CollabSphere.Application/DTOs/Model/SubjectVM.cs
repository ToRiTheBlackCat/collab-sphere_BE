using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Model
{
    public class SubjectVM
    {
        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public string SubjectCode { get; set; }

        public bool IsActive { get; set; }

        public SubjectSyllabusVM? SubjectSyllabus { get; set; }

        public static implicit operator SubjectVM(Subject subject)
        {
            if (subject == null) throw new ArgumentNullException();

            return new SubjectVM()
            {
                SubjectId = subject.SubjectId,
                SubjectName = subject.SubjectName,
                SubjectCode = subject.SubjectCode,
                IsActive = subject.IsActive,
                SubjectSyllabus = (SubjectSyllabusVM)subject.SubjectSyllabi.FirstOrDefault(),
            };
        }
    }
}
