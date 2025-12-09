using CollabSphere.Application.Base;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Student.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands.CreateTeam
{
    public class CreateTeamHandler : CommandHandler<CreateTeamCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateTeamHandler> _logger;

        public CreateTeamHandler(IUnitOfWork unitOfWork,
                                 ILogger<CreateTeamHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task<CommandResult> HandleCommand(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            int maxAdded = 5;
            int addedCount = 0;
            StringBuilder rawMessage = new StringBuilder();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                #region Create Team
                //Find lecturer
                var foundLecturer = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(request.LecturerId);

                // Create new team
                var newTeam = new Domain.Entities.Team
                {
                    TeamName = request.TeamName,
                    EnrolKey = request.EnrolKey ?? GenerateRandomEnrolKey(6),
                    Description = request.Description,
                    GitLink = request.GitLink,
                    LeaderId = request.LeaderId,
                    ClassId = request.ClassId,
                    ProjectAssignmentId = request.ProjectAssignmentId,
                    LecturerId = request.LecturerId,
                    LecturerName = foundLecturer?.Lecturer.Fullname,
                    CreatedDate = request.CreatedDate,
                    EndDate = request.EndDate,
                    Status = (int)TeamStatus.ACTIVE
                };
                await _unitOfWork.TeamRepo.Create(newTeam);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                #region Update team count in class
                // Add count for team in class
                var foundClass = await _unitOfWork.ClassRepo.GetClassByIdAsync(request.ClassId);
                foundClass.TeamCount++;
                _unitOfWork.ClassRepo.Update(foundClass);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                #region Update Class member in class
                //Update class member
                var foundLeader = await _unitOfWork.UserRepo.GetOneByUserIdAsync(request.LeaderId);
                var foundClassMember = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(request.ClassId, request.LeaderId);

                if (foundClassMember != null)
                {
                    foundClassMember.TeamId = newTeam.TeamId;
                    foundClassMember.TeamRole = (int?)TeamRole.LEADER;
                    foundClassMember.IsGrouped = true;

                    _unitOfWork.ClassMemberRepo.Update(foundClassMember);
                    await _unitOfWork.SaveChangesAsync();
                }
                #endregion

                #region Auto create team milestone
                //Create Team Milestone if picked project
                var foundProjectAssign = newTeam.ProjectAssignment;
                if (foundProjectAssign != null)
                {
                    var foundProject = await _unitOfWork.ProjectRepo.GetProjectDetail(foundProjectAssign.ProjectId);
                    if (foundProject != null)
                    {
                        //Find objective milestone
                        var objectives = foundProject.Objectives;
                        if (objectives.Any() || objectives.Count() > 0)
                        {
                            //Find milestones of objective
                            foreach (var obj in objectives)
                            {
                                var objMilestones = obj.ObjectiveMilestones;
                                if (objMilestones.Any() || objMilestones.Count() > 0)
                                {
                                    foreach (var mile in objMilestones)
                                    {
                                        //Create team milestone
                                        var newTeamMilestone = new TeamMilestone
                                        {
                                            ObjectiveMilestoneId = mile.ObjectiveMilestoneId,
                                            Title = mile.Title,
                                            Description = mile.Description,
                                            TeamId = newTeam.TeamId,
                                            StartDate = mile.StartDate,
                                            EndDate = mile.EndDate,
                                            Progress = 0,
                                            Status = (int)MilestoneStatus.NOTDONE,
                                        };

                                        await _unitOfWork.TeamMilestoneRepo.Create(newTeamMilestone);
                                        await _unitOfWork.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Add team memberss
                foreach (var student in request.StudentList)
                {
                    if (maxAdded > 0)
                    {
                        //Find classmember
                        var foundClassMem = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByClassIdAndStudentId(student.ClassId, student.StudentId);

                        //If not in class
                        if (foundClassMem == null)
                        {
                            rawMessage.Append($"Cannot add student with id: {student.StudentId} to team. This student not in this class with id: {student.ClassId} | ");
                            continue;
                        }
                        //If in class
                        else
                        {
                            //Check if already in any team
                            if (foundClassMem.TeamId != null)
                            {
                                //Same team
                                if (foundClassMem.TeamId == newTeam.TeamId)
                                {
                                    rawMessage.Append($"Cannot add this student because this student with id: {student.StudentId} already in this team. | ");
                                }
                                //Other team
                                else
                                {
                                    rawMessage.Append($"Cannot add this student with id: {student.StudentId} already in other team with id: {foundClassMem.TeamId}. | ");
                                }
                                continue;
                            }
                            //If not in any team
                            else
                            {
                                foundClassMem.TeamId = newTeam.TeamId;
                                foundClassMem.TeamRole = (int)TeamRole.MEMBER;
                                foundClassMem.IsGrouped = true;

                                _unitOfWork.ClassMemberRepo.Update(foundClassMem);
                                await _unitOfWork.SaveChangesAsync();

                                addedCount++;
                                maxAdded--;
                            }
                        }
                    }
                    //Full of member in team
                    else
                    {
                        result.IsSuccess = false;
                        rawMessage.Append($"Reach the max members of team, cannot add anymore. Fail to added student with id: {student.StudentId} into team with id: {newTeam.TeamId}| ");
                    }
                }
                #endregion

                #region Create Team WorkSpace
                var newTeamWkSpace = new TeamWorkspace
                {
                    TeamId = newTeam.TeamId,
                    Title = $"{newTeam.TeamName}'s WorkSpace",
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                };

                await _unitOfWork.TeamWorkspaceRepo.Create(newTeamWkSpace);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                #region Create Default Team Conversations
                var currentTime = DateTime.UtcNow;

                // User enities for students
                var newConversationUsers = new List<CollabSphere.Domain.Entities.User>() { foundLeader! };
                foreach (var studentInfo in request.StudentList)
                {
                    var studentUser = await _unitOfWork.UserRepo.GetOneByUserIdAsync(studentInfo.StudentId);
                    newConversationUsers.Add(studentUser!);
                }

                // Conversation with lecturer
                var lecturerConversation = new ChatConversation()
                {
                    ConversationName = $"{newTeam.TeamName} - {foundClass!.ClassName}",
                    ClassId = foundClass.ClassId,
                    TeamId = newTeam.TeamId,
                    Users = newConversationUsers.Append(foundLecturer).ToList(),
                    CreatedAt = currentTime,
                };

                // Private conversation between members (without lecturer)
                var privateConversation = new ChatConversation()
                {
                    ConversationName = $"{newTeam.TeamName} (Private) - {foundClass!.ClassName}",
                    ClassId = foundClass.ClassId,
                    TeamId = newTeam.TeamId,
                    Users = newConversationUsers.ToList(),
                    CreatedAt = currentTime,
                };

                await _unitOfWork.ChatConversationRepo.Create(lecturerConversation);
                await _unitOfWork.ChatConversationRepo.Create(privateConversation);
                await _unitOfWork.SaveChangesAsync();
                #endregion

                #region Create default team whiteboard & page
                var newWhiteboard = new Domain.Entities.TeamWhiteboard
                {
                    TeamId = newTeam.TeamId,
                    CreatedAt = DateTime.UtcNow,
                };
                await _unitOfWork.TeamWhiteboardRepo.Create(newWhiteboard);
                await _unitOfWork.SaveChangesAsync();

                //Create default page 
                var newPage = new WhiteboardPage
                {
                    WhiteboardId = newWhiteboard.WhiteboardId,
                    PageTitle = "Page1",
                    CreatedAt = DateTime.UtcNow,
                    IsActivate = true,
                };
                await _unitOfWork.WhiteboardPageRepo.Create(newPage);
                await _unitOfWork.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while creating team.");
            }

            await _unitOfWork.CommitTransactionAsync();
            rawMessage.Append($"Team created successfully.Add total {addedCount} students into team");
            result.IsSuccess = true;
            result.Message = rawMessage.ToString();

            return result;
        }

        private string GenerateRandomEnrolKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using var rng = RandomNumberGenerator.Create();
            var result = new char[length];
            var buffer = new byte[sizeof(uint)];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint num = BitConverter.ToUInt32(buffer, 0);
                result[i] = chars[(int)(num % (uint)chars.Length)];
            }

            return new string(result);
        }

        protected override async System.Threading.Tasks.Task ValidateRequest(List<OperationError> errors, CreateTeamCommand request)
        {
            var bypassRoles = new int[] { RoleConstants.LECTURER };
            if (bypassRoles.Contains(request.UserRole))
            {

                //Validate leaderId
                var foundLeader = await _unitOfWork.StudentRepo.GetStudentById(request.LeaderId);
                if (foundLeader == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.LeaderId),
                        Message = $"Not found any student with that Id: {request.LeaderId}"
                    });
                }

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
                else if (foundClass != null && !foundClass.IsActive)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.ClassId),
                        Message = $"Class with Id: {request.ClassId} is unactivate. Cannot use this function"
                    });

                }

                var foundSemester = foundClass.Semester;
                if(request.EndDate != null && foundSemester != null)
                {
                    if(request.EndDate < foundSemester.StartDate || request.EndDate > foundSemester.EndDate)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.EndDate),
                            Message = $"End date of team must be within the semester duration: {foundSemester.StartDate} - {foundSemester.EndDate}."
                        });
                    }
                }

                //Validate lecturer
                var foundLecturer = await _unitOfWork.LecturerRepo.GetById(request.LecturerId);
                if (foundLecturer == null)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.LecturerId),
                        Message = $"Not found any lecturer with that Id: {request.LeaderId}"
                    });
                }

                //Check if lecturer is assigned to that class
                if (foundClass?.LecturerId != request.UserId || foundClass?.LecturerId != request.LecturerId)
                {
                    errors.Add(new OperationError
                    {
                        Field = nameof(request.LecturerId),
                        Message = $"Lecturer with Id: {request.LecturerId} is not belong to class with Id: {request.ClassId}. Cannot create team"
                    });
                }

                //Check if input list > 0
                if (request.StudentList == null || request.StudentList.Count == 0)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "StudentList",
                        Message = $"Cannot add empty student list into team."
                    });
                    return;
                }

                //find existed project assignment
                var foundAssign = await _unitOfWork.ProjectAssignmentRepo.GetById(request.ProjectAssignmentId);
                if (foundAssign == null)
                {
                    errors.Add(new OperationError()
                    {
                        Field = "ProjectAssignmentId",
                        Message = $"Not found any project assignment with that {request.ProjectAssignmentId}"
                    });
                    return;
                }
                //Check if the project is assigned to that class
                else
                {
                    if (foundAssign.ClassId != request.ClassId)
                    {
                        errors.Add(new OperationError()
                        {
                            Field = "ProjectAssignmentId",
                            Message = $"Project Assignment with ID {request.ProjectAssignmentId} does not belong to Class {request.ClassId}"
                        });
                        return;
                    }
                }
            }
            else
            {
                errors.Add(new OperationError()
                {
                    Field = "UserRole",
                    Message = $"You do not have permission to create team."
                });
                return;
            }
        }
    }
}
