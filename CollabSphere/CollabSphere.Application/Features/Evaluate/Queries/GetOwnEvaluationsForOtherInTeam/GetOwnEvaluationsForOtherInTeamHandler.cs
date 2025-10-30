﻿using CollabSphere.Application.Base;
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

namespace CollabSphere.Application.Features.Evaluate.Queries.GetOwnEvaluationsForOtherInTeam
{
    public class GetOwnEvaluationsForOtherInTeamHandler : QueryHandler<GetOwnEvaluationsForOtherInTeamQuery, GetOwnEvaluationsForOtherInTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetOwnEvaluationsForOtherInTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<GetOwnEvaluationsForOtherInTeamResult> HandleCommand(GetOwnEvaluationsForOtherInTeamQuery request, CancellationToken cancellationToken)
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
                    var ownEvaluations = await _unitOfWork.MemberEvaluationRepo.GetEvaluationsOfOwnByUser(request.TeamId, request.UserId);
                    if (ownEvaluations != null)
                    {
                        var dtoList = new List<GetOwnEvaluationsForOtherInTeamDto>();

                        foreach (var x in ownEvaluations)
                        {
                            var user = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(x.Key);
                            dtoList.Add(new GetOwnEvaluationsForOtherInTeamDto
                            {
                                ReceiverId = x.Key,
                                ReceiverName = user?.Student.Fullname,
                                ScoreDetails = x.Value.Select(e => new ScoreDetail
                                {
                                    ScoreDetailName = e.Comment,
                                    Score = (int)e.Score
                                }).ToList()
                            });
                        }


                        result.OwnEvaluations = dtoList;
                        result.IsSuccess = true;
                        result.Message = $"Get evaluations and feedback for own with ID: {request.UserId} for others successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetOwnEvaluationsForOtherInTeamQuery request)
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
