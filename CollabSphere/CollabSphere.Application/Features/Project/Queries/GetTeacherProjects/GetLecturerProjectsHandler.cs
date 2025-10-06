using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Project.Queries.GetTeacherProjects
{
    public class GetLecturerProjectsHandler : QueryHandler<GetLecturerProjectsQuery, GetLecturerProjectResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetLecturerProjectsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetLecturerProjectResult> HandleCommand(GetLecturerProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetLecturerProjectResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                var projects = (await _unitOfWork.ProjectRepo.GetAll())
                    // Filter projects
                    .Where(x => 
                        x.LecturerId == request.LecturerId &&
                        (!request.Statuses.Any() || request.Statuses.Contains(x.Status)))
                    .Select(x => (ProjectVM)x)
                    .ToList();

                result.Projects = projects;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetLecturerProjectsQuery request)
        {
            var lecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);
            if (lecturer == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.LecturerId),
                    Message = $"Invalid lecturer ID: {request.LecturerId}",
                });
            }

            return;
        }
    }
}
