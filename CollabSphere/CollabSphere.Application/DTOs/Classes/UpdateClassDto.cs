using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.DTOs.Classes
{
    public class UpdateClassDto
    {
        public int ClassId = -1;

        [StringLength(100, MinimumLength = 3)]
        public string? ClassName { get; set; }

        public int? SubjectId { get; set; }

        public int? SemesterId { get; set; }

        public int? LecturerId { get; set; }

        [StringLength(20, MinimumLength = 3)]
        public string? EnrolKey { get; set; }

        public bool? IsActive { get; set; }
    }
}
