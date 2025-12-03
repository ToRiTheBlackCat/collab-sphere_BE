using CollabSphere.Application.Base;
using CollabSphere.Application.Common;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CollabSphere.Application.Features.Evaluate.Commands.LecEvaluateTeamMilestone
{
    public class LecturerEvaluateTeamMilestoneHandler : CommandHandler<LecturerEvaluateTeamMilestoneCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configure;
        private readonly EmailSender _emailSender;
        public LecturerEvaluateTeamMilestoneHandler(IUnitOfWork unitOfWork, IConfiguration configure)
        {
            _unitOfWork = unitOfWork;
            _configure = configure;
            _emailSender = new EmailSender(_configure);
        }
        protected override async Task<CommandResult> HandleCommand(LecturerEvaluateTeamMilestoneCommand request, CancellationToken cancellationToken)
        {
            var result = new CommandResult()
            {
                IsSuccess = false,
                IsValidInput = true,
                Message = string.Empty
            };
            var receiverEmails = new HashSet<string>();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
                if (foundTeamMilestone != null)
                {
                    var foundTeam = await _unitOfWork.TeamRepo.GetById(foundTeamMilestone.TeamId);
                    if (foundTeam != null)
                    {
                        //Get team member to send mail
                        var teamMembers = await _unitOfWork.ClassMemberRepo.GetClassMemberAsyncByTeamId(foundTeam.TeamId);
                        //Get the receiver email
                        foreach (var member in teamMembers!)
                        {
                            var foundStu = await _unitOfWork.UserRepo.GetOneByUIdWithInclude(member.StudentId);
                            receiverEmails.Add(foundStu!.Email);
                        }

                        //Check existed milestone evaluation
                        var foundMileEvaluation = await _unitOfWork.MilestoneEvaluationRepo.GetEvaluationOfMilestone(request.TeamMilestoneId, request.EvaluatorId, foundTeam.TeamId);

                        //If existed - updated
                        if (foundMileEvaluation != null)
                        {
                            foundMileEvaluation.Score = request.Score;
                            foundMileEvaluation.Comment = request.Comment;
                            foundMileEvaluation.CreatedDate = DateTime.UtcNow;

                            _unitOfWork.MilestoneEvaluationRepo.Update(foundMileEvaluation);
                            await _unitOfWork.SaveChangesAsync();

                            //Send mail
                            await _emailSender.SendNotiEmailsForMileEva(receiverEmails, foundTeam.TeamName, foundTeamMilestone.Title, foundMileEvaluation);
                            result.Message = $"Update milestone evaluation for team milestone with ID: {foundMileEvaluation.MilestoneId} successfully";
                        }
                        else
                        {
                            //Create new evaluation
                            var newMileEvaluation = new MilestoneEvaluation
                            {
                                MilestoneId = request.TeamMilestoneId,
                                LecturerId = request.EvaluatorId,
                                TeamId = foundTeam.TeamId,
                                Score = request.Score,
                                Comment = request.Comment,
                                CreatedDate = DateTime.UtcNow
                            };
                            await _unitOfWork.MilestoneEvaluationRepo.Create(newMileEvaluation);
                            await _unitOfWork.SaveChangesAsync();
                            //Send mail
                            await _emailSender.SendNotiEmailsForMileEva(receiverEmails, foundTeam.TeamName, foundTeamMilestone.Title, newMileEvaluation);
                            result.Message = $"Evaluate for team milestone with ID: {request.TeamMilestoneId} successfully";
                        }

                    }
                    await _unitOfWork.CommitTransactionAsync();
                    result.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        protected override async Task ValidateRequest(List<OperationError> errors, LecturerEvaluateTeamMilestoneCommand request)
        {
            //Find team milestone
            var foundTeamMilestone = await _unitOfWork.TeamMilestoneRepo.GetTeamMilestoneById(request.TeamMilestoneId);
            if (foundTeamMilestone == null)
            {
                errors.Add(new OperationError
                {
                    Field = nameof(request.TeamMilestoneId),
                    Message = $"Not found any Team Milestone with that Id: {request.TeamMilestoneId}"
                });
                return;
            }
            else
            {
                var foundTeam = await _unitOfWork.TeamRepo.GetById(foundTeamMilestone.TeamId);
                if (foundTeam != null)
                {
                    //Check if lecturer of team
                    if (foundTeam.LecturerId != request.EvaluatorId)
                    {
                        errors.Add(new OperationError
                        {
                            Field = nameof(request.EvaluatorId),
                            Message = $"You are not the lecturer of this team. Cannot use this function"
                        });
                        return;
                    }
                }
            }
        }
    }
}
