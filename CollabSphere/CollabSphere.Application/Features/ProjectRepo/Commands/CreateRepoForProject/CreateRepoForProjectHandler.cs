using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Commands.CreateTeam;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.ProjectRepo.Commands.CreateRepoForProject
{
    public class CreateRepoForProjectHandler : CommandHandler<CreateRepoForProjectCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateRepoForProjectHandler (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(CreateRepoForProjectCommand request, CancellationToken cancellationToken)
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

                var foundProject = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
                if (foundProject != null)
                {
                    var newProjectRepo = new Domain.Entities.ProjectRepository
                    {
                        ProjectId = request.ProjectId,
                        GithubRepoId = request.GithubRepoId,
                        RepositoryName = request.RepositoryName,
                        RepositoryUrl = request.RepositoryUrl,
                        WebhookId = request.WebhookId,
                        ConnectedByUserId = request.ConnectedByUserId,
                        ConnectedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ProjectRepo_Repo.Create(newProjectRepo);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    result.IsSuccess = true;
                    result.Message = $"Create repo for project with ID: {request.ProjectId} successfully";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateRepoForProjectCommand request)
        {
            //Find project
            var foundProject = await _unitOfWork.ProjectRepo.GetById(request.ProjectId);
            if (foundProject == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ProjectId),
                    Message = $"Not found any project with that Id: {request.ProjectId}"
                });
                return;
            }

            //Find user
            var foundUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(request.ConnectedByUserId);
            if (foundUser == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ConnectedByUserId),
                    Message = $"Not found any user with that Id: {request.ConnectedByUserId}"
                });
                return;
            }
        }
    }
}
