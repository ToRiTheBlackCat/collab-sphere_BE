using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Team.Commands.UpdateTeam;
using CollabSphere.Application.Features.TeamMilestones.Commands.UpdateTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Queries.GetMilestoneDetail;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.TeamMilestones
{
    public class UpdateTeamMilestoneTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneRepoMock;

        private readonly UpdateTeamMilestoneHandler _handler;

        public UpdateTeamMilestoneTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _teamMilestoneRepoMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneRepoMock.Object);

            _handler = new UpdateTeamMilestoneHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateMilestone_WhenValidCommand()
        {
            // Arrange
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = new Application.DTOs.TeamMilestones.UpdateTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    StartDate = new DateOnly(2021, 2, 11),
                    EndDate = new DateOnly(2021, 2, 23),
                },
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                    },
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 2",
                        Description = "Check second description.",
                        StartDate = new DateOnly(2021, 2, 21),
                        DueDate = new DateOnly(2021, 2, 23),
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(2021, 2, 12),
                EndDate = new DateOnly(2021, 2, 23),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotLecturerOfClass()
        {
            // Arrange
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = new Application.DTOs.TeamMilestones.UpdateTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    StartDate = new DateOnly(2021, 2, 11),
                    EndDate = new DateOnly(2021, 2, 23),
                },
                UserId = 16,
                UserRole = RoleConstants.LECTURER
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                    },
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 2",
                        Description = "Check second description.",
                        StartDate = new DateOnly(2021, 2, 21),
                        DueDate = new DateOnly(2021, 2, 23),
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(2021, 2, 12),
                EndDate = new DateOnly(2021, 2, 23),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the assigned lecturer", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenStartDateAfterEndDate()
        {
            // Arrange
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = new Application.DTOs.TeamMilestones.UpdateTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    StartDate = new DateOnly(2021, 2, 11),
                    EndDate = new DateOnly(2021, 2, 10),
                },
                UserId = 16,
                UserRole = RoleConstants.LECTURER
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                    },
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 2",
                        Description = "Check second description.",
                        StartDate = new DateOnly(2021, 2, 21),
                        DueDate = new DateOnly(2021, 2, 23),
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(2021, 2, 12),
                EndDate = new DateOnly(2021, 2, 23),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the assigned lecturer", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenStartDateAfterFirstCheckpointStartDate()
        {
            // Arrange
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = new Application.DTOs.TeamMilestones.UpdateTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    StartDate = new DateOnly(2021, 2, 13),
                    EndDate = new DateOnly(2021, 2, 23),
                },
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                    },
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 2",
                        Description = "Check second description.",
                        StartDate = new DateOnly(2021, 2, 21),
                        DueDate = new DateOnly(2021, 2, 23),
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(2021, 2, 12),
                EndDate = new DateOnly(2021, 2, 23),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("StartDate can't be a date later than earliest", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenEndDateBeforeLatestCheckpointEndDate()
        {
            // Arrange
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = new Application.DTOs.TeamMilestones.UpdateTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    StartDate = new DateOnly(2021, 2, 12),
                    EndDate = new DateOnly(2021, 2, 19),
                },
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                    },
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 2",
                        Description = "Check second description.",
                        StartDate = new DateOnly(2021, 2, 21),
                        DueDate = new DateOnly(2021, 2, 23),
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(2021, 2, 12),
                EndDate = new DateOnly(2021, 2, 23),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("EndDate can't be a date earlier than latest", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new UpdateTeamMilestoneCommand()
            {
                TeamMilestoneDto = new Application.DTOs.TeamMilestones.UpdateTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    StartDate = new DateOnly(2021, 2, 12),
                    EndDate = new DateOnly(2021, 2, 23),
                },
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                Title = "Team 1 Milestone 1",
                Description = "Description of Milestone 1",
                TeamId = 1,
                Team = new Domain.Entities.Team()
                {
                    Class = new Class()
                    {
                        ClassId = 1,
                        LecturerId = 8,
                    },
                    ClassMembers = new List<ClassMember>()
                    {

                    },
                },
                Checkpoints = new List<Checkpoint>()
                {
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 1",
                        Description = "Check first description.",
                        StartDate = new DateOnly(2021, 2, 12),
                        DueDate = new DateOnly(2021, 2, 20),
                    },
                    new Checkpoint()
                    {
                        CheckpointId = 1,
                        Title = "Checkpoint 2",
                        Description = "Check second description.",
                        StartDate = new DateOnly(2021, 2, 21),
                        DueDate = new DateOnly(2021, 2, 23),
                    },
                },
                MilestoneQuestions = new List<MilestoneQuestion>()
                {

                },
                MilestoneEvaluation = null,
                MilestoneFiles = new List<MilestoneFile>()
                {

                },
                MilestoneReturns = new List<MilestoneReturn>()
                {

                },
                Progress = 87,
                StartDate = new DateOnly(2021, 2, 12),
                EndDate = new DateOnly(2021, 2, 23),
                Status = (int)TeamMilestoneStatuses.NOT_DONE,
            };
            _teamMilestoneRepoMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);
            _teamMilestoneRepoMock.Setup(x => x.Update(It.IsAny<TeamMilestone>())).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains("DB Exception", result.Message);

            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
