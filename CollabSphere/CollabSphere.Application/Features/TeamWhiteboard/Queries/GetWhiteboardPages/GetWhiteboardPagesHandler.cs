using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardByTeamId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.TeamWhiteboard.Queries.GetWhiteboardPages
{
    public class GetWhiteboardPagesHandler : QueryHandler<GetWhiteboardPagesQuery, GetWhiteboardPagesResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWhiteboardPagesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetWhiteboardPagesResult> HandleCommand(GetWhiteboardPagesQuery request, CancellationToken cancellationToken)
        {
            var result = new GetWhiteboardPagesResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundWhiteboard = await _unitOfWork.TeamWhiteboardRepo.GetById(request.WhiteboardId);
                if (foundWhiteboard != null)
                {
                    var whiteboardPages = await _unitOfWork.WhiteboardPageRepo.GetPagesOfWhiteboard(request.WhiteboardId);

                    result.Pages = whiteboardPages;
                }
                await _unitOfWork.CommitTransactionAsync();
                result.IsSuccess = true;
                result.Message = $"Get pages of whiteboard with ID: {request.WhiteboardId} successfully";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetWhiteboardPagesQuery request)
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
