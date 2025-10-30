using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.DeleteCustomTeamMilestone;
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
    public class DeleteTeamMilestoneTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneRepoMock;

        private readonly DeleteCustomTeamMilestoneHandler _handler;

        public DeleteTeamMilestoneTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _teamMilestoneRepoMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneRepoMock.Object);

            _handler = new DeleteCustomTeamMilestoneHandler(_unitOfWorkMock.Object);
        }

        public void SetupMocks()
        {
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
                        LecturerId = 88,
                    },
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassId = 1,
                            ClassMemberId = 33,
                            StudentId = 3,
                        },
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
            _teamMilestoneRepoMock.Setup(x => x.GetDetailsById(10)).ReturnsAsync(milestone);
        }

        [Fact]
        public async Task Handle_ShouldDeleteTeamMilestone_WhenValidCommand()
        {
            // Arrange
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = 10,
                UserId = 88,
                UserRole = RoleConstants.LECTURER,
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Deleted successfully team milestone", result.Message);

            _teamMilestoneRepoMock.Verify(x =>
                x.Update(It.Is<TeamMilestone>(x =>
                    x.Status == (int)TeamMilestoneStatuses.SOFT_DELETED
                )),
                Times.Once
            );
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = 10,
                UserId = 88,
                UserRole = RoleConstants.LECTURER,
            };

            this.SetupMocks();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Empty(result.ErrorList);
            Assert.Contains("DB Exception", result.Message);

            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenMilestoneNotFound()
        {
            // Arrange
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = 11,
                UserId = 88,
                UserRole = RoleConstants.LECTURER,
            };

            this.SetupMocks();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No team milestone with ID '11'.", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenIsNotLecturerOfClass()
        {
            // Arrange
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = 10,
                UserId = 77,
                UserRole = RoleConstants.LECTURER,
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the assigned lecturer of the class", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenUserIsStudent()
        {
            // Arrange
            var command = new DeleteCustomTeamMilestoneCommand()
            {
                TeamMilestoneId = 10,
                UserId = 77,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the assigned lecturer of the class", result.ErrorList.First().Message);
        }
    }
}
