using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Commands
{
    public class ImportStudentCommand : ICommand
    {
        public List<ImportStudentDto> StudentList { get; set; }
    }
}
