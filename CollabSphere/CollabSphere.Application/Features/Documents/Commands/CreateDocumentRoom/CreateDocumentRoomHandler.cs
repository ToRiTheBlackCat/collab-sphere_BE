using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Documents.Queries.GetTeamDocuments;
using CollabSphere.Application.Mappings.DocumentRooms;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Documents.Commands.CreateDocumentRoom
{
    public class CreateDocumentRoomHandler : CommandHandler<CreateDocumentRoomCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateDocumentRoomHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<CommandResult> HandleCommand(CreateDocumentRoomCommand request, CancellationToken cancellationToken)
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

                #region Data Operation
                var currentTime = DateTime.UtcNow;

                var docRoom = new DocumentRoom()
                {
                    TeamId = request.TeamId,
                    RoomName = request.RoomDto.RoomName,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime,
                };
                await _unitOfWork.DocRoomRepo.Create(docRoom);
                await _unitOfWork.SaveChangesAsync(); 
                #endregion

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = $"Created document room '{docRoom.RoomName}' successfully.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, CreateDocumentRoomCommand request)
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

            // Check for duplicated document RoomName
            var existingRooms = await _unitOfWork.DocRoomRepo.GetDocRoomsByTeam(request.TeamId);
            var duplicatedName = existingRooms.Any(x => 
                x.RoomName.Equals(request.RoomDto.RoomName, StringComparison.OrdinalIgnoreCase));
            if (duplicatedName)
            {
                errors.Add(new OperationError()
                {
                    Field = nameof(request.RoomDto.RoomName),
                    Message = $"Team '{team.TeamName}'({team.TeamId}) already have a document named '{request.RoomDto.RoomName}'.",
                });
                return;
            }
        }
    }
}
