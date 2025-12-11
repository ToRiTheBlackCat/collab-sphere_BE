using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using CollabSphere.Application.Features.Subjects.Commands.CreateSubject;
using System.ComponentModel.DataAnnotations;

namespace CollabSphere.Application.Features.Subjects.Commands.UpdateSubject
{
    public class UpdateSubjectCommand : ICommand
    {
        public int SubjectId { get; set; }

        public CreateSubjectDto Subject { get; set; } = null!;
    }
}
