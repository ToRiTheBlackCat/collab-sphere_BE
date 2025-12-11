using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using System.ComponentModel.DataAnnotations;

namespace CollabSphere.Application.Features.Subjects.Commands.CreateSubject
{
    public class CreateSubjectCommand : ICommand
    {
        public CreateSubjectDto Subject { get; set; } = null!;
    }
}
