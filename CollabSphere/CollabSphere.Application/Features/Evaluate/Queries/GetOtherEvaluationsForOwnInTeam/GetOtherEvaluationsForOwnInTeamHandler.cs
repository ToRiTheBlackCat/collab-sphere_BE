using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Evaluate;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam;
using CollabSphere.Application.Features.Evaluate.Queries.GetLecturerEvaluationForTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.GetOtherEvaluationsForOwnInTeam
{
    public class GetOtherEvaluationsForOwnInTeamHandler : QueryHandler<GetOtherEvaluationsForOwnInTeamQuery, GetOwnEvaluationsForOtherInTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;
        public GetOtherEvaluationsForOwnInTeamHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetOwnEvaluationsForOtherInTeamResult> HandleCommand(GetOtherEvaluationsForOwnInTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new GetOwnEvaluationsForOtherInTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };

            try
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam != null)
                {
                    var foundClassMember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, request.UserId);
                    var otherEvaluations = await _unitOfWork.MemberEvaluationRepo.GetEvaluationsForReceiver(request.TeamId, foundClassMember.ClassMemberId);
                    if (otherEvaluations != null)
                    {
                        var dtoList = new List<OtherEvaluationsForOwnInTeamDto>();

                        foreach (var x in otherEvaluations)
                        {
                            var foundMember = await _unitOfWork.ClassMemberRepo.GetById(x.Key);
                            var raterUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(foundMember.StudentId);
                            var raterName = raterUser?.Student?.Fullname ?? "";
                            var raterAvatar = (await _cloudinaryService.GetImageUrl(raterUser?.Student.AvatarImg));
                            var foundClasMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(foundTeam.TeamId, foundClassMember.StudentId);

                            var dto = new OtherEvaluationsForOwnInTeamDto
                            {
                                RaterId = x.Key,
                                RaterName = raterName,
                                RaterAvatar = raterAvatar,
                                RaterCode = raterUser.Student.StudentCode,
                                RaterTeamRole = foundClasMem?.TeamRole,
                                ScoreDetails = x.Value.Select(e => new ScoreDetail
                                {
                                    ScoreDetailName = e.Comment ?? string.Empty,
                                    Score = (int)e.Score
                                }).ToList()
                            };

                            dtoList.Add(dto);
                        }


                        result.OtherEvaluations = dtoList;
                        result.IsSuccess = true;
                        result.Message = $"Get evaluations and feedback for user with ID: {request.UserId} successfully";
                    }
                    else
                    {
                        result.OtherEvaluations = null;
                        result.IsSuccess = true;
                        result.Message = $"Not found any evaluation for this user with ID: {request.UserId} yet";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetOtherEvaluationsForOwnInTeamQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.STUDENT };

            //Check role permission
            if (!bypassRoles.Contains(request.UserRole))
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You not have permission to do this function"
                });
                return;
            }
            else
            {
                //Check if team exists
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null || foundTeam.Status == 0)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "TeamId",
                        Message = $"Team with the given ID: {request.TeamId} does not exist!"
                    });
                    return;
                }

                //Check if the requester is in the class
                var classmember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(foundTeam.ClassId, request.UserId);

                if (classmember == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.UserId),
                        Message = $"This student with ID: {request.UserId} not in class with ID: {foundTeam.ClassId}. Cannot use this function"
                    });
                    return;
                }
                else
                {
                    //Check if student has team in this class
                    if (classmember.TeamId == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"This student with ID: {request.UserId} not has any team in class. Cannot use this function"
                        });
                        return;
                    }
                    else if (classmember.TeamId != foundTeam.TeamId)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"This student with ID: {request.UserId} not in this team. Cannot use this function"
                        });
                        return;
                    }
                }
            }
        }
    }
}
