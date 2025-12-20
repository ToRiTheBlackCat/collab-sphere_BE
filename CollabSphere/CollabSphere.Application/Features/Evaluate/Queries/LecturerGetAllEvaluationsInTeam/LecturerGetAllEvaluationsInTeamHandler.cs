using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Evaluate;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Evaluate.Commands.StudentEvaluateOtherInTeam;
using CollabSphere.Application.Features.Evaluate.Queries.GetOtherEvaluationsForOwnInTeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Evaluate.Queries.LecturerGetAllEvaluationsInTeam
{
    public class LecturerGetAllEvaluationsInTeamHandler : QueryHandler<LecturerGetAllEvaluationsInTeamQuery, LecturerGetAllEvaluationsInTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public LecturerGetAllEvaluationsInTeamHandler(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<LecturerGetAllEvaluationsInTeamResult> HandleCommand(LecturerGetAllEvaluationsInTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new LecturerGetAllEvaluationsInTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if(foundTeam != null)
                {
                    var memberEvaluations = await _unitOfWork.MemberEvaluationRepo.GetMemberEvaluationsOfTeam(request.TeamId);
                    if(memberEvaluations != null && memberEvaluations.Count > 0)
                    {
                        var resultList = new List<MemberEvaluations>();

                        var allTeamMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(request.TeamId);

                        foreach (var receiverEntry in memberEvaluations)
                        {
                            var receiverId = receiverEntry.Key;
                            var rawEvaluations = receiverEntry.Value;

                            var receiverMember = allTeamMembers.FirstOrDefault(m => m.ClassMemberId == receiverId);
                            var receiverUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(receiverMember.StudentId); 

                            var receiverDto = new MemberEvaluations
                            {
                                ReceiverId = receiverId,
                                Evaluations = new List<OtherEvaluationsForOwnInTeamDto>()
                            };

                            var evaluationsByRater = rawEvaluations.GroupBy(e => e.RaterId);

                            foreach (var raterGroup in evaluationsByRater)
                            {
                                var raterId = raterGroup.Key;
                                var foundClassMem = await _unitOfWork.ClassMemberRepo.GetById(raterId);
                                var raterUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(foundClassMem.StudentId);
                                var raterMember = allTeamMembers.FirstOrDefault(m => m.ClassMemberId == raterId);
                                var raterAvatar = (await _cloudinaryService.GetImageUrl(raterUser?.Student.AvatarImg));
                                var evaluationDto = new OtherEvaluationsForOwnInTeamDto
                                {
                                    RaterId = raterId,
                                    RaterName = raterUser?.Student?.Fullname,
                                    RaterCode = raterUser?.Student?.StudentCode,
                                    RaterAvatar = await _cloudinaryService.GetImageUrl(raterUser?.Student?.AvatarImg),
                                    RaterTeamRole = raterMember?.TeamRole,

                                    ScoreDetails = raterGroup.Select(e => new ScoreDetail
                                    {
                                        ScoreDetailName = e.Comment ?? string.Empty, 
                                        Score = (int)e.Score
                                    }).ToList()
                                };

                                receiverDto.Evaluations.Add(evaluationDto);
                            }

                            resultList.Add(receiverDto);
                        }

                        result.MemberEvaluations = resultList;
                        result.IsSuccess = true;
                        result.Message = "Get member evaluations for this team successfully";
                    }
                    else
                    {
                        result.Message = "No evaluations found in the team.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, LecturerGetAllEvaluationsInTeamQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER };

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
                else
                {
                    if (foundTeam.LecturerId != request.UserId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserId",
                            Message = $"You are not the lecturer of the team. Cannot use this function"
                        });
                        return;
                    }
                }
            }
        }
    }
}
