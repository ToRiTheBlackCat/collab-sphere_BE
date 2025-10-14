using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectAssignments.Commands.AssignProjectsToClass
{
    public class AssignProjectsToClassHandler : CommandHandler<AssignProjectsToClassCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AssignProjectsToClassHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(AssignProjectsToClassCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Data Operations
                // Get current time
                var assignDate = DateTime.UtcNow;

                // Create new Project Assignments
                foreach (var projectId in request.ProjectIds)
                {
                    var newProjectAssign = new ProjectAssignment()
                    {
                        AssignedDate = assignDate,
                        ClassId = request.ClassId,
                        ProjectId = projectId,
                    };

                    await _unitOfWork.ProjectAssignmentRepo.Create(newProjectAssign);
                }
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Assigned {request.ProjectIds.Count} project(s) to class with ID '{request.ClassId}'.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, AssignProjectsToClassCommand request)
        {
            // Check ClassId
            var classEntity = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (classEntity == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.ClassId),
                    Message = $"No Class with ID: {request.ClassId}",
                });
                return;
            }

            // Check Projects
            var projectAssignment = await _unitOfWork.ProjectAssignmentRepo.GetProjectAssignmentsByClassAsync(request.ClassId);
            var assignedProjectIds = projectAssignment.Select(x => x.ProjectId).ToHashSet();
            for (int i = 0; i < request.ProjectIds.Count; i++)
            {
                var projectId = request.ProjectIds.ElementAt(i);
                var projectIdprefix = $"{nameof(request.ProjectIds)}[{i}]";

                var project = await _unitOfWork.ProjectRepo.GetById(projectId);
                if (project == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = projectIdprefix,
                        Message = $"No Project with ID: {projectId}",
                    });
                }
                else if (project.Status != (int)ProjectStatuses.APPROVED)
                {
                    errors.Add(new OperationError()
                    {
                        Field = projectIdprefix,
                        Message = $"Project with ID '{projectId}' is not {ProjectStatuses.APPROVED.ToString()}.",
                    });
                }
                else if (assignedProjectIds.Contains(projectId))
                {
                    {
                        errors.Add(new OperationError()
                        {
                            Field = projectIdprefix,
                            Message = $"Class with ID '{request.ClassId}' is already assigned Project with ID '{projectId}'.",
                        });
                    }
                }
            }
        }
    }
}
