using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.DTOs.Teams;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Queries.GetTeamDetail;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Student.Queries.GetStudentOfClass
{
    public class GetStudentOfClassHandler : QueryHandler<GetStudentOfClassQuery, GetStudentOfClassResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetStudentOfClassHandler> _logger;

        public GetStudentOfClassHandler(IUnitOfWork unitOfWork,
                                 ILogger<GetStudentOfClassHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<GetStudentOfClassResult> HandleCommand(GetStudentOfClassQuery request, CancellationToken cancellationToken)
        {
            var result = new GetStudentOfClassResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                //Find existed class
                var foundClass = await _unitOfWork.ClassRepo.GetClassByIdAsync(request.ClassId);
                if (foundClass == null)
                {
                    result.Message = "No class found for this classId.";
                    result.StudentsOfClass = null;
                    return result;
                }

                //Find members of class
                var classMembers = foundClass.ClassMembers.ToList();
                if (!classMembers.Any() || classMembers.Count == 0)
                {
                    result.Message = "This class not has any members.";
                    result.StudentsOfClass = null;
                    return result;
                }
                var dtoList = new List<StudentOfClassDto>();

                foreach (var member in classMembers)
                {
                    //Find existed student
                    var foundStudent = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(member.StudentId);
                    if (foundStudent == null)
                    {
                        continue;
                    }

                    //Map to DTO
                    var dto = new StudentOfClassDto
                    {
                        UId = member.StudentId,
                        Email = foundStudent.Email,
                        Fullname = foundStudent.Student.Fullname,
                        AvatarPublicId = foundStudent.Student.AvatarImg,
                        StudentCode = foundStudent.Student.StudentCode,
                        Major = foundStudent.Student.Major,
                        IsActive = foundStudent.IsActive,

                        //Detail info
                        DetailInfo = new DetailInfo
                        {
                            Address = foundStudent.Student.Address,
                            PhoneNumber = foundStudent.Student.PhoneNumber,
                            Yob = foundStudent.Student.Yob,
                            School = foundStudent.Student.School,
                        },

                        //Team info
                        TeamInfo = new TeamInfo
                        {
                            TeamId = member.TeamId,
                            TeamName = member.Team?.TeamName,
                            TeamRole = member.TeamRole != null 
                                ? member.TeamRole == 1 ?"Leader" :"Member" 
                                : null,
                        },

                        //Project info
                        ProjectInfo = new ProjectInfoOfStudent
                        {
                            ProjectAssignmentId = member.Team?.ProjectAssignmentId,
                            ProjectId = member.Team?.ProjectAssignment?.ProjectId,
                            ProjectName = member.Team?.ProjectAssignment?.Project.ProjectName
                        }
                    };
                    dtoList.Add(dto);
                }

                result.StudentsOfClass = new PagedList<StudentOfClassDto>(
                  list: dtoList,
                  pageNum: request.PageNum,
                  pageSize: request.PageSize,
                  viewAll: request.ViewAll
              );

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting student list of class with id: {ClassId}", request.ClassId);
                result.Message = "An error occurred while processing your request.";
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, GetStudentOfClassQuery request)
        {
            var bypassRoles = new int[] { RoleConstants.STAFF, RoleConstants.LECTURER, RoleConstants.STUDENT };

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
                //Check class existed
                var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
                if (foundClass == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "ClassId",
                        Message = $"Class with the given ID: {request.ClassId} does not exist!"
                    });
                    return;
                }

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
                    if (request.UserId != foundClass.LecturerId)
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This lecturer with ID: {request.UserId} not has permission to get student list of this class."
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

                    //Check if student is in that class
                    var studentInClass = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(request.ClassId, request.UserId);

                    if (studentInClass == null)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "UserRole",
                            Message = $"This student with ID: {request.UserId} not has permission to view student list of this class."
                        });
                    }
                }
                #endregion
            }
        }
    }
}
