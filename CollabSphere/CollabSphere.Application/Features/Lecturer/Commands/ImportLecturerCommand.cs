using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Lecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Lecturer.Commands
{
    public class ImportLecturerCommand : ICommand
    {
        public List<ImportLecturerDto> LecturerList { get; set; }
    }
}
