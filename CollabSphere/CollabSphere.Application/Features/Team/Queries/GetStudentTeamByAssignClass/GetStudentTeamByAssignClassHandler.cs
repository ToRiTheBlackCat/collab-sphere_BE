using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetAllTeamByAssignClass;
using CollabSphere.Application.Mappings.Team;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetStudentTeamByAssignClass
{
    public class GetStudentTeamByAssignClassHandler : QueryHandler<GetStudentTeamByAssignClassQuery, GetStudentTeamByAssignClassResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetStudentTeamByAssignClassHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;

        public GetStudentTeamByAssignClassHandler(IUnitOfWork unitOfWork,
                                              ILogger<GetStudentTeamByAssignClassHandler> logger,
                                              CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetStudentTeamByAssignClassResult> HandleCommand(GetStudentTeamByAssignClassQuery request, CancellationToken cancellationToken)
        {
            var result = new GetStudentTeamByAssignClassResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetListTeamOfStudent(request.UserId, null, request.ClassId, null);
                if (foundTeam == null || foundTeam.Count == 0)
                {
                    result.IsSuccess = true;
                    result.Message = $"Not found any team for student ID {request.UserId} in class ID {request.ClassId}";
                    return result;
                }

                var mapppedTeam = (foundTeam.FirstOrDefault()).Team_To_StudentTeamByAssignClassDto();
                mapppedTeam.TeamImage = await _cloudinaryService.GetImageUrl(foundTeam.FirstOrDefault().TeamImage);


                result.StudentTeam = mapppedTeam;
                result.IsSuccess = true;
                result.Message = "Get team of student successfully.";
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while getting teams for class ID {ClassId}", request.ClassId);
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetStudentTeamByAssignClassQuery request)
        {
            //Validate class
            var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
            if (foundClass == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.ClassId),
                    Message = $"Not found any class with that Id: {request.ClassId}"
                });
            }
            else
            {
                //Check role permission
                if (request.UserRole != RoleConstants.STUDENT)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserRole),
                        Message = $"This user with ID: {request.UserId} not has permission to get list of team of this class."
                    });
                }
                else
                {
                    var classmember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(request.ClassId, request.UserId);

                    if (classmember == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"This student with ID: {request.UserId} not in class with ID: {request.ClassId}."
                        });
                    }
                    else
                    {
                        //Check if student has team in this class
                        if (classmember.TeamId == null)
                        {
                            errors.Add(new OperationError
                            {
                                Field = nameof(request.UserId),
                                Message = $"This student with ID: {request.UserId} not has any team in class with ID: {request.ClassId}."
                            });
                        }
                    }
                }
            }
        }
    }
}
