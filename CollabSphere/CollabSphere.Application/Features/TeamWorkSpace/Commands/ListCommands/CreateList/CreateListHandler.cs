using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWorkSpace.Commands.ListCommands.CreateList
{
    public class CreateListHandler : CommandHandler<CreateListCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateListHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<CommandResult> HandleCommand(CreateListCommand request, CancellationToken cancellationToken)
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

                var newList = new List
                {
                    Title = request.Title,
                    Position = request.Position,
                    WorkspaceId = request.WorkSpaceId,
                };

                await _unitOfWork.ListRepo.Create(newList);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var createdList = await _unitOfWork.ListRepo.GetById(newList.ListId);

                result.IsSuccess = true;
                result.Message = JsonSerializer.Serialize(createdList);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.IsSuccess = false;
            }

            return result;
        }

        protected override async System.Threading.Tasks.Task ValidateRequest(List<OperationError> errors, CreateListCommand request)
        {
            //Find workspace
            var foundWorkspace = await _unitOfWork.TeamWorkspaceRepo.GetById(request.WorkSpaceId);
            if (foundWorkspace == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.WorkSpaceId),
                    Message = $"Not found any workspace with ID: {request.WorkSpaceId}"
                });
                return;
            }

            //Find user
            var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.RequesterId);
            if (foundUser == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.RequesterId),
                    Message = $"Not found any user with ID: {request.RequesterId}"
                });
                return;
            }

            //Validate position
            if (request.Position <= 0.0)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.Position),
                    Message = $"Cannot input position <= 0"
                });
                return;
            }
        }
    }
}
