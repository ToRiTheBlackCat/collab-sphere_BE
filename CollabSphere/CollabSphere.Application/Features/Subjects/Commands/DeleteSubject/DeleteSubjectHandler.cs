using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Subjects.Commands.DeleteSubject
{
    public class DeleteSubjectHandler : CommandHandler<DeleteSubjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteSubjectHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteSubjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {

            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var subject = await _unitOfWork.SubjectRepo.GetSubjectDetail(request.SubjectId);

                var syllabus = subject!.SubjectSyllabi.First();

                foreach (var outcomes in syllabus.SubjectOutcomes.ToList())
                {
                    foreach (var syllabusMilestone in outcomes.SyllabusMilestones.ToList())
                    {
                        _unitOfWork.SyllabusMilestoneRepo.Delete(syllabusMilestone);
                    }

                    _unitOfWork.SubjectOutcomeRepo.Delete(outcomes);
                }

                foreach(var gradeComponent in syllabus.SubjectGradeComponents.ToList())
                {
                    _unitOfWork.SubjectGradeComponentRepo.Delete(gradeComponent);
                }

                await _unitOfWork.SaveChangesAsync();

                _unitOfWork.SubjectSyllabusRepo.Delete(syllabus);
                await _unitOfWork.SaveChangesAsync();

                _unitOfWork.SubjectRepo.Delete(subject);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted Subject '{subject.SubjectName}'({subject.SubjectId}) successfully.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteSubjectCommand request)
        {
            var subjectDetail = await _unitOfWork.SubjectRepo.GetSubjectDetail(request.SubjectId);
            if (subjectDetail == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.SubjectId),
                    Message = $"No Subject with ID '{request.SubjectId}' found.",
                });
                return;
            }

            // Can not delete a subject that is already implemented by classes
            var subjectClasses = await _unitOfWork.ClassRepo.SearchClasses(subjectIds: new HashSet<int>() { request.SubjectId });
            if (subjectClasses.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.SubjectId),
                    Message = $"Can not delete Subject '{subjectDetail!.SubjectName}'({subjectDetail.SubjectId}) because it is implemented {subjectClasses.Count} by classe(s).",
                });
            }

            var subjectProjects = await _unitOfWork.ProjectRepo.SearchProjects(subjectIds: new List<int> { request.SubjectId });
            if (subjectProjects.Any())
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.SubjectId),
                    Message = $"Can not delete Subject '{subjectDetail!.SubjectName}'({subjectDetail.SubjectId}) because it is implemented by {subjectProjects.Count} subject(s).",
                });
            }
        }
    }
}
