using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.UpdateTeamMilestone;
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
    public class CheckTeamMilestoneTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneRepoMock;

        private readonly CheckTeamMilestoneHandler _handler;

        public CheckTeamMilestoneTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _teamMilestoneRepoMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneRepoMock.Object);

            _handler = new CheckTeamMilestoneHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCheckDoneMilestone_WhenValidCommand()
        {
            // Arrange
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = new Application.DTOs.TeamMilestones.CheckTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    IsDone = true
                },
                UserId = 3,
                UserRole = (int)RoleConstants.STUDENT
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
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 3,
                            TeamRole = (int)TeamRole.LEADER
                        }
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

            _teamMilestoneRepoMock.Verify(
                x => x.Update(It.Is<TeamMilestone>(x => x.TeamMilestoneId == 10 && x.Status == (int)TeamMilestoneStatuses.DONE)),
                Times.Once
            );
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(),Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(),Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCheckUnDoneMilestone_WhenValidCommand()
        {
            // Arrange
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = new Application.DTOs.TeamMilestones.CheckTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    IsDone = false
                },
                UserId = 3,
                UserRole = (int)RoleConstants.STUDENT
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
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 3,
                            TeamRole = (int)TeamRole.LEADER
                        }
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

            _teamMilestoneRepoMock.Verify(
                x => x.Update(It.Is<TeamMilestone>(x => x.TeamMilestoneId == 10 && x.Status == (int)TeamMilestoneStatuses.NOT_DONE)),
                Times.Once
            );
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenMilestoneNotFound()
        {
            // Arrange
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = new Application.DTOs.TeamMilestones.CheckTeamMilestoneDto()
                {
                    TeamMilestoneId = 13,
                    IsDone = false
                },
                UserId = 3,
                UserRole = (int)RoleConstants.STUDENT
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
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 3,
                            TeamRole = (int)TeamRole.LEADER
                        }
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
            Assert.Contains("No team milestone with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotLeaderOfTeam()
        {
            // Arrange
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = new Application.DTOs.TeamMilestones.CheckTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    IsDone = false
                },
                UserId = 3,
                UserRole = (int)RoleConstants.STUDENT
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
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 3,
                            TeamRole = (int)TeamRole.MEMBER
                        }
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
            Assert.Contains("not the LEADER the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotMemberOfTeam()
        {
            // Arrange
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = new Application.DTOs.TeamMilestones.CheckTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    IsDone = false
                },
                UserId = 4,
                UserRole = (int)RoleConstants.STUDENT
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
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 3,
                            TeamRole = (int)TeamRole.MEMBER
                        }
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
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new CheckTeamMilestoneCommand()
            {
                CheckDto = new Application.DTOs.TeamMilestones.CheckTeamMilestoneDto()
                {
                    TeamMilestoneId = 10,
                    IsDone = false
                },
                UserId = 3,
                UserRole = (int)RoleConstants.STUDENT
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
                        new ClassMember()
                        {
                            ClassMemberId = 1,
                            StudentId = 3,
                            TeamRole = (int)TeamRole.LEADER
                        }
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

            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(),Times.Once);
        }
    }
}
