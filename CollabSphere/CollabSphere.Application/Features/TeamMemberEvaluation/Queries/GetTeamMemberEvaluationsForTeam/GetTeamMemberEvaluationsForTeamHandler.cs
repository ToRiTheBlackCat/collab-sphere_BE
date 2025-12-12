using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.TeamMemEvaluation;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.TeamMemberEvaluation.Queries.GetTeamMemberEvaluationsForTeam
{
    public class GetTeamMemberEvaluationsForTeamHandler : QueryHandler<GetTeamMemberEvaluationsForTeamQuery, GetTeamMemberEvaluationsForTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private Domain.Entities.Team _foundTeam;
        private Domain.Entities.ClassMember _ownScore;

        public GetTeamMemberEvaluationsForTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task<GetTeamMemberEvaluationsForTeamResult> HandleCommand(GetTeamMemberEvaluationsForTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new GetTeamMemberEvaluationsForTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                (int? lecturerId, int? studentId) ids = request.UserRole switch
                {
                    RoleConstants.LECTURER => ((int?)request.UserId, null),
                    RoleConstants.STUDENT => (null, (int?)request.UserId),
                    _ => (null, null)
                };
                var (lecturerId, classMemberId) = ids;

                //Find the team member evaluations
                var teamMemberEvaluations = await _unitOfWork.TeamMemEvaluationRepo.GetTeamMemEvaluations(request.TeamId, ids.lecturerId, ids.studentId);
                var dto = new TeamMemEvaluationDto
                {
                    LecturerId = _foundTeam.LecturerId,
                    TeamId = _foundTeam.TeamId,
                    MemberScores = new List<MemberEvaluationDto>()
                };

                if (teamMemberEvaluations.Count > 0)
                {
                    //Loop through each team member evaluation
                    foreach (var member in teamMemberEvaluations)
                    {
                        dto.MemberScores.Add(new MemberEvaluationDto
                        {
                            ClassMemberId = member.ClassMemberId,
                            MemberName = (await _unitOfWork.ClassMemberRepo.GetById(member.ClassMemberId))?.Fullname,
                            Score = member.Score,
                        });
                    }
                }
                else if (teamMemberEvaluations?.Count == 0)
                {
                    //If get own score of student
                    if (ids.studentId != null)
                    {
                        dto.MemberScores.Add(new MemberEvaluationDto
                        {
                            ClassMemberId = _ownScore.ClassMemberId,
                            MemberName = (await _unitOfWork.ClassMemberRepo.GetById(_ownScore.ClassMemberId))?.Fullname,
                            Score = null,
                        });
                    }
                    //If Lecturer get all members score
                    else
                    {
                        var teamMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(_foundTeam.TeamId);
                        foreach (var member in teamMembers)
                        {
                            dto.MemberScores.Add(new MemberEvaluationDto
                            {
                                ClassMemberId = member.ClassMemberId,
                                MemberName = (await _unitOfWork.ClassMemberRepo.GetById(member.ClassMemberId))?.Fullname,
                                Score = null,
                            });
                        }
                    }
                }

                result.TeamMemEvaluations = dto;
                result.IsSuccess = true;
                result.Message = $"Get score of members in this team successfully";
            }
            catch (Exception ex)
            {
                result.Message = $"Fail to get score of members in this team";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetTeamMemberEvaluationsForTeamQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };
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
                if (foundTeam == null || foundTeam.Status == (int)TeamStatus.DEACTIVE)
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
                    _foundTeam = foundTeam;
                }

                //If Lecturer
                if (request.UserRole == RoleConstants.LECTURER)
                {
                    if (request.UserId != foundTeam.LecturerId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserId",
                            Message = $"You are not the lecturer of this team. Cannot view member score of this team"
                        });
                        return;
                    }
                }
                //If Student
                else
                {
                    var classmember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamIdAndStudentId(request.TeamId, request.UserId);

                    if (classmember == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"This student is not in this team. Cannot use this function"
                        });
                        return;
                    }
                    _ownScore = classmember;
                }
            }
        }
    }
}
