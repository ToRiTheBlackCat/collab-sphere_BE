using Amazon.S3;
using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Mappings.DocumentRooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Documents.Queries.GetTeamDocuments
{
    public class GetTeamDocumentsHandler : QueryHandler<GetTeamDocumentsQuery, GetTeamDocumentsResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTeamDocumentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetTeamDocumentsResult> HandleCommand(GetTeamDocumentsQuery request, CancellationToken cancellationToken)
        {
            var result = new GetTeamDocumentsResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };  

            try
            {
                var documentRooms = await _unitOfWork.DocRoomRepo.GetDocRoomsByTeam(request.TeamId);
                result.DocumentRooms = documentRooms.ToViewModels();

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetTeamDocumentsQuery request)
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
        }
    }
}
