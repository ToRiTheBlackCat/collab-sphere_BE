using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Queries.GetTeamDetail
{
    public class GetTeamDetailHandler : QueryHandler<GetTeamDetailQuery, GetTeamDetailResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTeamDetailHandler> _logger;
        private readonly CloudinaryService _cloudinaryService;

        public GetTeamDetailHandler(IUnitOfWork unitOfWork,
                                 ILogger<GetTeamDetailHandler> logger,
                                 CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task<GetTeamDetailResult> HandleCommand(GetTeamDetailQuery request, CancellationToken cancellationToken)
        {
            var result = new GetTeamDetailResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var foundTeam = await _unitOfWork.TeamRepo.GetTeamDetail(request.TeamId);
                var foundSemester = await _unitOfWork.SemesterRepo.GetById(foundTeam.Class.SemesterId);
                #region Map to DTO
                var dto = new TeamDetailDto
                {
                    TeamId = foundTeam.TeamId,
                    TeamName = foundTeam.TeamName,
                    TeamImage = await _cloudinaryService.GetImageUrl(foundTeam.TeamImage),
                    SemesterName = foundSemester.SemesterName,
                    EnrolKey = foundTeam.EnrolKey ?? string.Empty,
                    Description = foundTeam.Description ?? string.Empty,
                    GitLink = foundTeam.GitLink ?? string.Empty,
                    CreatedDate = foundTeam.CreatedDate,
                    EndDate = foundTeam.EndDate,
                    Status = foundTeam.Status,

                    // Class Info
                    ClassInfo = new ClassInfo
                    {
                        ClassId = foundTeam.Class?.ClassId ?? 0,
                        ClassName = foundTeam.Class?.ClassName ?? string.Empty
                    },

                    // Lecturer Info
                    LecturerInfo = new LecturerInfo
                    {
                        LecturerId = foundTeam.Class?.Lecturer?.LecturerId ?? 0,
                        LecturerName = foundTeam.Class?.Lecturer?.Fullname ?? string.Empty
                    },

                    // Project Info
                    ProjectInfo = new ProjectInfo
                    {
                        ProjectAssignmentId = foundTeam.ProjectAssignment?.ProjectAssignmentId,
                        ProjectId = foundTeam.ProjectAssignment?.Project?.ProjectId,
                        ProjectName = foundTeam.ProjectAssignment?.Project?.ProjectName ?? string.Empty,
                        ProjectDescription = foundTeam.ProjectAssignment?.Project?.Description ?? string.Empty
                    }

                };
                //Member Info
                var members = foundTeam.ClassMembers?
                    .Select(cm => new TeamMemberInfo
                    {
                        StudentId = cm.Student.StudentId,
                        StudentName = cm.Student.Fullname,
                        Avatar = cm.Student.AvatarImg, // store temp image path
                        TeamRole = cm.TeamRole,
                        MemberContributionPercentage = 0
                    }).ToList() ?? new List<TeamMemberInfo>();

                // Resolve avatar URLs in parallel safely
                foreach (var member in members)
                {
                    member.Avatar = await _cloudinaryService.GetImageUrl(member.Avatar);
                }

                dto.MemberInfo = new MemberInfo
                {
                    MemberCount = members.Count,
                    Members = members
                };


                // Calculate Progress Info 
                var milestones = foundTeam.TeamMilestones?.ToList() ?? new List<Domain.Entities.TeamMilestone>();
                var totalMilestones = milestones.Count;
                var completedMilestones = milestones.Count(m => m.Status == 1);

                var checkpoints = milestones.SelectMany(m => m.Checkpoints ?? new List<Domain.Entities.Checkpoint>()).ToList();
                var totalCheckpoints = checkpoints.Count;
                var completedCheckpoints = checkpoints.Count(c => c.Status == 1);



                dto.TeamProgress = new TeamProgress
                {
                    //Total progress
                    OverallProgress = (totalMilestones + totalCheckpoints) > 0
                        ? (float)Math.Round(
                            ((completedMilestones + completedCheckpoints) * 100f) /
                            (totalMilestones + totalCheckpoints), 2)
                        : 0,
                    //Milestone
                    MilestonesProgress = totalMilestones > 0
                        ? (float)Math.Round((completedMilestones * 100f / totalMilestones), 2)
                        : 0,
                    TotalMilestones = totalMilestones,
                    MilestonesComplete = completedMilestones,
                    //Checkpoint
                    CheckPointProgress = totalCheckpoints > 0
                        ? (float)Math.Round((completedCheckpoints * 100f / totalCheckpoints), 2)
                        : 0,
                    TotalCheckpoints = totalCheckpoints,
                    CheckpointsComplete = completedCheckpoints,
                };
                #endregion

                result.TeamDetail = dto;
                result.IsSuccess = true;
                result.Message = "Successfully retrieved team details.";

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while getting detail of team with ID {TeamId}", request.TeamId);
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetTeamDetailQuery request)
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
                    #region If Lecturer
                    if (request.UserRole == RoleConstants.LECTURER)
                    {
                        //Check if lecturer exists
                        var foundLecturer = await _unitOfWork.LecturerRepo.GetById(request.UserId);
                        if (foundLecturer == null)
                        {
                            errors.Add(new OperationError()
                            {
                                Field = "UserId",
                                Message = $"Lecturer with the given ID: {request.UserId} does not exist."
                            });
                        }
                        //Check if lecturer is the owner of the team
                        if (request.UserId != foundTeam.LecturerId)
                            errors.Add(new OperationError()
                            {
                                Field = "UserRole",
                                Message = $"This lecturer with ID: {request.UserId} not has permission to get this team details."
                            });
                    }
                    #endregion
                    #region If Student
                    else if (request.UserRole == RoleConstants.STUDENT)
                    {
                        //Check if student exists
                        var foundStudent = await _unitOfWork.StudentRepo.GetById(request.UserId);
                        if (foundStudent == null)
                        {
                            errors.Add(new OperationError()
                            {
                                Field = "UserId",
                                Message = $"Student with the given ID: {request.UserId} does not exist."
                            });
                        }

                        //Check if student is in the team
                        var studentInClass = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(request.TeamId);

                        if (studentInClass == null || !studentInClass.Any(x => x.StudentId == request.UserId))
                        {
                            errors.Add(new OperationError()
                            {
                                Field = "UserRole",
                                Message = $"This student with ID: {request.UserId} not has permission to get this team details."
                            });
                        }
                    }
                    #endregion
                }
            }
        }
    }
}
