using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.PrAnalysis.Queries.GetListOfAnalysis
{
    public class GetListAnalysisOfTeamHandler : QueryHandler<GetListAnalysisOfTeamQuery, GetListAnalysisOfTeamResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetListAnalysisOfTeamHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected override async Task<GetListAnalysisOfTeamResult> HandleCommand(GetListAnalysisOfTeamQuery request, CancellationToken cancellationToken)
        {
            var result = new GetListAnalysisOfTeamResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty,
                AnalysisDetail = new AnalysisDetailDto()
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var (totalItems, analysisList) = await _unitOfWork.PrAnalysisRepo.GetListOfAnalysisByTeamIdAndRepoId(request.TeamId, request.RepositoryId, request.CurrentPage, request.PageSize, request.IsDesc);

                if (totalItems != 0 && analysisList != null)
                {
                    #region Find team - Map to DTO
                    var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                    if (foundTeam != null)
                    {
                        var teamInfoDto = new TeamInfo
                        {
                            Id = foundTeam.TeamId,
                            Name = foundTeam.TeamName
                        };

                        result.AnalysisDetail.TeamInfo = teamInfoDto;
                    }
                    #endregion
                    # region Find Repo - Map to DTO
                    var foundRepo = await _unitOfWork.ProjectRepoMappingRepo.GetOneByTeamIdAndRepoId(request.TeamId, request.RepositoryId);
                    if (foundRepo != null)
                    {
                        var repoInfoDto = new RepositoryInfo
                        {
                            Id = foundRepo.RepositoryId,
                            FullName = foundRepo.RepositoryFullName
                        };

                        result.AnalysisDetail.RepositoryInfo = repoInfoDto;
                    }
                    #endregion
                    #region Create PaginationDTO
                    int totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
                    var paginationDto = new PaginationDto
                    {
                        IsSuccess = true,
                        CurrentPage = request.CurrentPage,
                        PageSize = request.PageSize,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        Items = new()
                    };
                    #region Create ItemDto
                    foreach (var analysis in analysisList)
                    {
                        var newItemDto = new Item
                        {
                            AnalysisId = analysis.Id,
                            PrNumber = analysis.PrNumber,
                            PrTitle = analysis.PrTitle,
                            PrAuthor = analysis.PrAuthorGithubUsername,
                            PrUrl = analysis.PrUrl,
                            AiSummary = analysis.AiSummary,
                            AiScore = analysis.AiOverallScore,
                            BugCount = analysis.AiBugCount,
                            SecurityIssueCount = analysis.AiSecurityIssueCount,
                            AnalyzedAt = analysis.AnalyzedAt
                        };

                        paginationDto.Items.Add(newItemDto);
                    }
                    #endregion
                    result.AnalysisDetail.Pagination = paginationDto;
                    #endregion

                    result.IsSuccess = true;
                    result.Message = "Get list anaslysis successfully";
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Not found any analysis of this teamId: {request.TeamId} | repoId: {request.RepositoryId}";
                }

            }
            catch (Exception ex)
            {
                result.Message = $"Fail to get list anaslysis successfully. Error detail: {ex.Message}";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetListAnalysisOfTeamQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER, RoleConstants.STUDENT };

            //Check role permission
            if (bypassRoles.Contains(request.UserRole))
            {
                //Find user
                if (request.UserId != 0)
                {
                    var foundUser = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.UserId);

                    if (foundUser == null)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.UserId),
                            Message = $"Not found any user with that Id: {request.UserId}"
                        });
                        return;
                    }
                }
                //Find repo of team
                var foundRepo = await _unitOfWork.ProjectRepoMappingRepo.GetOneByTeamIdAndRepoId(request.TeamId, request.RepositoryId);
                if (foundRepo == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.RepositoryId),
                        Message = $"Not found any repos of team with that RepoId: {request.RepositoryId}"
                    });
                    return;
                }
                //Find team
                var foundTeam = await _unitOfWork.TeamRepo.GetById(request.TeamId);
                if (foundTeam == null || foundTeam.Status == 0)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.TeamId),
                        Message = $"Not found any team with that Id: {request.TeamId}"
                    });
                    return;
                }
                else
                {
                    //Check user in team
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
            else
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.UserRole),
                    Message = $"Your role do not have permisison to use this function"
                });
                return;
            }
        }

    }
}

