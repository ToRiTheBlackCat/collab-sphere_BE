using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Commands.DeleteDocumentRoom
{
    public class DeleteDocumentRoomHandler : CommandHandler<DeleteDocumentRoomCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDocumentRoomHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(DeleteDocumentRoomCommand request, CancellationToken cancellationToken)
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
                var docRoom = (await _unitOfWork.DocRoomRepo.GetDocumentRoom(request.TeamId, request.RoomName))!;

                // Document states of room
                var docStates = await _unitOfWork.DocStateRepo.GetStatesByDocumentRoom(request.TeamId, request.RoomName);
                foreach (var state in docStates)
                {
                    _unitOfWork.DocStateRepo.Delete(state);
                }
                await _unitOfWork.SaveChangesAsync();

                _unitOfWork.DocRoomRepo.Delete(docRoom);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.Message = $"Deleted document room '{docRoom.RoomName}' successfully.";
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, DeleteDocumentRoomCommand request)
        {
            // Get team
            var team = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
            if (team == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.TeamId),
                    Message = $"No Team with ID '{request.TeamId}' found.",
                });
                return;
            }

            // Requester is Lecturer
            if (request.UserRole == RoleConstants.LECTURER)
            {
                // Check if is class's assigned lecturer
                if (request.UserId != team.Class.LecturerId)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not the assigned lecturer of the class with ID '{team.Class.ClassId}'.",
                    });
                    return;
                }
            }
            // Requester is Student
            else if (request.UserRole == RoleConstants.STUDENT)
            {
                // Check if is member of team
                var isTeamMember = team.ClassMembers.Any(x => x.StudentId == request.UserId);
                if (!isTeamMember)
                {
                    errors.Add(new OperationError()
                    {
                        Field = nameof(request.UserId),
                        Message = $"You ({request.UserId}) are not a member of the team with ID '{team.TeamId}'.",
                    });
                    return;
                }
            }

            // Check if document exist
            var docRoom = await _unitOfWork.DocRoomRepo.GetDocumentRoom(request.TeamId, request.RoomName);
            if (docRoom == null)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(docRoom.RoomName),
                    Message = $"No Document of  with ID '{request.RoomName}' found in the team '{team.TeamName}'({team.TeamId}).",
                });
                return;
            }
        }
    }
}
