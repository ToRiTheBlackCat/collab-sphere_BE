using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.TeamWhiteboard.Commands.CreatePage
{
    public class CreatePageHandler : CommandHandler<CreatePageCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePageHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreatePageCommand request, CancellationToken cancellationToken)
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

                var foundWhiteboard = await _unitOfWork.TeamWhiteboardRepo.GetById(request.WhiteboardId);
                if (foundWhiteboard != null)
                {
                    var newPage = new WhiteboardPage
                    {
                        WhiteboardId = request.WhiteboardId,
                        PageTitle = request.PageTitle.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        IsActivate = true,
                    };

                    await _unitOfWork.WhiteboardPageRepo.Create(newPage);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    result.IsSuccess = true;
                    result.Message = JsonSerializer.Serialize(newPage, jsonOptions);
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreatePageCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check role permission
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"This role with ID: {request.UserRole} not has permission to get this team details."
                });
                return;
            }
            else
            {
                //Check if whiteboard exists
                var foundWhiteboard = await _unitOfWork.TeamWhiteboardRepo.GetById(request.WhiteboardId);
                if (foundWhiteboard == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "WhiteboardId",
                        Message = $"Cannot find any whiteboard with ID: {request.WhiteboardId}"
                    });
                    return;
                }
            }
        }
    }
}
