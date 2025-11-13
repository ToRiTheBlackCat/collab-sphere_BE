using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassHandler : CommandHandler<UpdateClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateClassHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(UpdateClassCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var classDto = request.ClassDto;
                var classEntity = (await _unitOfWork.ClassRepo.GetById(classDto.ClassId))!;

                await _unitOfWork.BeginTransactionAsync();

                #region Data Operation
                if (!string.IsNullOrWhiteSpace(classDto.ClassName))
                {
                    classEntity.ClassName = request.ClassDto.ClassName;
                }

                if (classDto.LecturerId.HasValue)
                {
                    classEntity.Lecturer = null;

                    var lecturer = await _unitOfWork.LecturerRepo.GetById(classDto.LecturerId.Value);
                    classEntity.LecturerId = lecturer!.LecturerId;
                    classEntity.LecturerName = lecturer.Fullname;

                    foreach(var team in classEntity.Teams)
                    {
                        team.ProjectAssignment = null;

                        team.LecturerId = lecturer.LecturerId;
                        team.LecturerName = lecturer.Fullname;
                    }
                }

                if (classDto.SubjectId.HasValue && classEntity.SubjectId != classDto.SubjectId)
                {
                    classEntity.Subject = null;
                    classEntity.SubjectId = classDto.SubjectId.Value;
                }

                if (classDto.SemesterId.HasValue && classEntity.SemesterId != classDto.SemesterId)
                {
                    classEntity.Semester = null;
                    classEntity.SemesterId = classDto.SemesterId.Value;
                }

                if (!string.IsNullOrWhiteSpace(classDto.EnrolKey))
                {
                    classEntity.EnrolKey = classDto.EnrolKey;
                }

                if (classDto.IsActive.HasValue)
                {
                    classEntity.IsActive = classDto.IsActive.Value;
                }

                _unitOfWork.ClassRepo.Update(classEntity);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Updated class with ID: {classEntity.ClassId}";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, UpdateClassCommand request)
        {
            // Check class existence
            var classEntity = await _unitOfWork.ClassRepo.GetById(request.ClassDto.ClassId);
            if (classEntity == null)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.ClassDto.ClassId)}",
                    Message = $"No class with ID: {request.ClassDto.ClassId}",
                });
                return;
            }

            var classDto = request.ClassDto;
            // Check updated subjectId
            if (classDto.SubjectId.HasValue && classDto.SubjectId != classEntity.SubjectId)
            {
                var subject = await _unitOfWork.SubjectRepo.GetById(classDto.SubjectId.Value);
                if (subject == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(classDto.SubjectId)}",
                        Message = $"No subject with ID: {classDto.SubjectId}",
                    });
                }

            }

            // Check updated semesterId
            if (classDto.SemesterId.HasValue && classDto.SemesterId != classEntity.SemesterId)
            {
                var semester = await _unitOfWork.SemesterRepo.GetById(classDto.SemesterId.Value);
                if (semester == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(classDto.SemesterId)}",
                        Message = $"No semester with ID: {classDto.SemesterId}",
                    });
                }
            }

            // Check updated lecturerId
            if (classDto.LecturerId.HasValue && classDto.LecturerId != classEntity.SubjectId)
            {
                var lecturer = await _unitOfWork.LecturerRepo.GetById(classDto.LecturerId.Value);

                if (lecturer == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(request.ClassDto.LecturerId)}",
                        Message = $"No lecturer with ID: {request.ClassDto.LecturerId}",
                    }); 
                }
            }
        }
    }
}
