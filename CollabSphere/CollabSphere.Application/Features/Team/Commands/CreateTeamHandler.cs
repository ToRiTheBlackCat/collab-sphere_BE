using CollabSphere.Application.Base;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Student.Commands;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Application.Features.Team.Commands
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
            try
            {
                await _unitOfWork.BeginTransactionAsync();

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
                    Status = request.Status
                };
                await _unitOfWork.TeamRepo.Create(newTeam);
                await _unitOfWork.SaveChangesAsync();

                // Add count for team in class
                var foundClass = await _unitOfWork.ClassRepo.GetById(request.ClassId);
                foundClass.TeamCount++;
                _unitOfWork.ClassRepo.Update(foundClass);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                result.IsSuccess = true;
                result.Message = "Team created successfully.";
                _logger.LogInformation("Team created successfully with ID: {TeamId}", newTeam.TeamId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Message = ex.Message;
                _logger.LogError(ex, "Error occurred while creating team.");
            }
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

        protected override async Task ValidateRequest(List<OperationError> errors, CreateTeamCommand request)
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
            if (foundClass?.LecturerId != request.LecturerId)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.LecturerId),
                    Message = $"Lecturer with Id: {request.LecturerId} is not belong to class with Id: {request.ClassId}"
                });
            }
        }
    }
}
