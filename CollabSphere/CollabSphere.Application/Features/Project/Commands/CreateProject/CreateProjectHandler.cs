using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Project.Commands.CreateProject
{
    public class CreateProjectHandler : CommandHandler<CreateProjectCommand, CreateProjectResult>
    {
        private readonly IUnitOfWork _uniUnitOfWork;

        public CreateProjectHandler(IUnitOfWork uniUnitOfWork)
        {
            _uniUnitOfWork = uniUnitOfWork;
        }

        protected override async Task<CreateProjectResult> HandleCommand(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var result = new CreateProjectResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _uniUnitOfWork.BeginTransactionAsync();

                #region Data Operations
                var newPoject = request.Project.ToProjectEntity();
                newPoject.CreatedAt = DateTime.UtcNow;
                newPoject.UpdatedAt = DateTime.UtcNow;
                newPoject.UpdatedBy = request.UserId;

                await _uniUnitOfWork.ProjectRepo.Create(newPoject);
                await _uniUnitOfWork.SaveChangesAsync();
                #endregion

                await _uniUnitOfWork.CommitTransactionAsync();

                result.ProjectId = newPoject.ProjectId;
                result.Message = "Project Created Successfully.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _uniUnitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateProjectCommand request)
        {
            var projectDto = request.Project;

            // Check Requester's Lecturer ID
            if (request.UserId != projectDto.LecturerId)
            {
                errors.Add(new OperationError()
                {
                    Field = $"{nameof(request.UserId)}",
                    Message = $"UserId ({request.UserId}) doesn't match the Project's LecturerId ({projectDto.LecturerId}).",
                });
            }

            // Check Lecturer ID
            var lecturer = await _uniUnitOfWork.LecturerRepo.GetById(projectDto.LecturerId);
            if (lecturer == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(projectDto.LecturerId),
                    Message = $"No existing Lecturer with this ID: {projectDto.LecturerId}",
                });
            }

            // Check Subject ID
            var subject = await _uniUnitOfWork.SubjectRepo.GetById(projectDto.SubjectId);
            if (subject == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(projectDto.SubjectId),
                    Message = $"No existing Subject with this ID: {projectDto.SubjectId}",
                });
            }

            return;
        }
    }
}
