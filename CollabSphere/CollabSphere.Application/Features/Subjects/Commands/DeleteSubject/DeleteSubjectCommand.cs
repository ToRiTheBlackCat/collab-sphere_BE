using CollabSphere.Application.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectCommand : ICommand
    {
        public int SubjectId { get; set; }
    }
}
