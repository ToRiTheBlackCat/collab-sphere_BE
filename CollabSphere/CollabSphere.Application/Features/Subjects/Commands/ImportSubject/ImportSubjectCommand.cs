using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Subjects.Commands.ImportSubject
{
    public class ImportSubjectCommand : ICommand
    {
        public List<ImportSubjectDto> Subjects { get; set; } = new List<ImportSubjectDto>();
    }
}
