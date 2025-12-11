using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
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
                //var newPoject = request.Project.ToProjectEntity();
                var newPoject = new Domain.Entities.Project()
                {
                    ProjectName = request.Project.ProjectName,
                    Description = request.Project.Description,
                    LecturerId = request.Project.LecturerId,
                    SubjectId = request.Project.SubjectId,
                    Status = (int)ProjectStatuses.PENDING,
                    BusinessRules = request.Project.BusinessRules,
                    Actors = request.Project.Actors,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = request.UserId
                };

                await _uniUnitOfWork.ProjectRepo.Create(newPoject);
                await _uniUnitOfWork.SaveChangesAsync();
                #endregion

                await _uniUnitOfWork.CommitTransactionAsync();

                result.ProjectId = newPoject.ProjectId;
                result.Message = $"Created project '{newPoject.ProjectName}'({newPoject.ProjectId}) successfully.";
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

            var bypassRoles = new List<int>()
            {
                RoleConstants.HEAD_DEPARTMENT,
                RoleConstants.STAFF
            };

            if (!bypassRoles.Contains(request.UserRole))
            {
                // Check Requester's Lecturer ID
                if (request.UserId != projectDto.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = $"{nameof(request.UserId)}",
                        Message = $"UserId ({request.UserId}) doesn't match the Project's LecturerId ({projectDto.LecturerId}).",
                    });
                }
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
