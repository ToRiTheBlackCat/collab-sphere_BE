using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Checkpoints.Commands.CreateCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Queries.GetCheckpointDetail;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Checkpoints
{
    public class CreateCheckpointTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICheckpointRepository> _checkpointRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneMock;

        private readonly CreateCheckpointHandler _handler;

        public CreateCheckpointTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _checkpointRepoMock = new Mock<ICheckpointRepository>();
            _teamMilestoneMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.CheckpointRepo).Returns(_checkpointRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneMock.Object);

            _handler = new CreateCheckpointHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateCheckpoint_WhenValidCommand()
        {
            // Arrange
            var command = new CreateCheckpointCommand()
            {
                TeamMilestoneId = 10,
                Title = "Checkpoint Name",
                Description = "Checkpoint Description",
                Complexity = "HIGH",
                StartDate = new DateOnly(2025, 10, 21),
                DueDate = new DateOnly(2025, 10, 26),
                UserId = 8,
                UserRole = RoleConstants.STUDENT
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 11,
                StartDate = new DateOnly(2025, 10, 20),
                EndDate = new DateOnly(2025, 10, 26),
                Team = new Domain.Entities.Team()
                {
                    TeamId = 11,
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 3,
                            StudentId = 8
                        }
                    },
                },
            };
            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Created checkpoint successfully", result.Message);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _checkpointRepoMock.Verify(x => 
                x.Create(
                    It.Is<Checkpoint>(x => 
                        x.TeamMilestoneId == 10 &&
                        x.Title.Equals("Checkpoint Name") &&
                        x.Description.Equals("Checkpoint Description") &&
                        x.Complexity.Equals("HIGH") &&
                        x.StartDate == new DateOnly(2025, 10, 21) && 
                        x.DueDate == new DateOnly(2025, 10, 26) &&
                        x.Status == (int)CheckpointStatuses.NOT_DONE
                    )
                ), 
                Times.Once
            );
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotMemberOfTeam()
        {
            // Arrange
            var command = new CreateCheckpointCommand()
            {
                TeamMilestoneId = 10,
                Title = "Checkpoint Name",
                Description = "Checkpoint Description",
                Complexity = "HIGH",
                StartDate = new DateOnly(2025, 10, 21),
                DueDate = new DateOnly(2025, 10, 26),
                UserId = 7,
                UserRole = RoleConstants.STUDENT
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 11,
                StartDate = new DateOnly(2025, 10, 20),
                EndDate = new DateOnly(2025, 10, 26),
                Team = new Domain.Entities.Team()
                {
                    TeamId = 11,
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 3,
                            StudentId = 8
                        }
                    },
                },
            };
            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenStartDateAfterDueDate()
        {
            // Arrange
            var command = new CreateCheckpointCommand()
            {
                TeamMilestoneId = 10,
                Title = "Checkpoint Name",
                Description = "Checkpoint Description",
                Complexity = "HIGH",
                StartDate = new DateOnly(2025, 10, 21),
                DueDate = new DateOnly(2025, 10, 26),
                UserId = 7,
                UserRole = RoleConstants.STUDENT
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 11,
                StartDate = new DateOnly(2025, 10, 20),
                EndDate = new DateOnly(2025, 10, 26),
                Team = new Domain.Entities.Team()
                {
                    TeamId = 11,
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 3,
                            StudentId = 8
                        }
                    },
                },
            };
            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenStartDateNotInMilestoneDateRange()
        {
            // Arrange
            var command = new CreateCheckpointCommand()
            {
                TeamMilestoneId = 10,
                Title = "Checkpoint Name",
                Description = "Checkpoint Description",
                Complexity = "HIGH",
                StartDate = new DateOnly(2025, 10, 19),
                DueDate = new DateOnly(2025, 10, 26),
                UserId = 8,
                UserRole = RoleConstants.STUDENT
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 11,
                StartDate = new DateOnly(2025, 10, 20),
                EndDate = new DateOnly(2025, 10, 26),
                Team = new Domain.Entities.Team()
                {
                    TeamId = 11,
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 3,
                            StudentId = 8
                        }
                    },
                },
            };
            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("StartDate can't be a date before milestone's StartDate", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenDueDateNotInMilestoneDateRange()
        {
            // Arrange
            var command = new CreateCheckpointCommand()
            {
                TeamMilestoneId = 10,
                Title = "Checkpoint Name",
                Description = "Checkpoint Description",
                Complexity = "HIGH",
                StartDate = new DateOnly(2025, 10, 21),
                DueDate = new DateOnly(2025, 10, 28),
                UserId = 8,
                UserRole = RoleConstants.STUDENT
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 11,
                StartDate = new DateOnly(2025, 10, 20),
                EndDate = new DateOnly(2025, 10, 26),
                Team = new Domain.Entities.Team()
                {
                    TeamId = 11,
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 3,
                            StudentId = 8
                        }
                    },
                },
            };
            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("DueDate can't be a date after milestone's EndDate", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new CreateCheckpointCommand()
            {
                TeamMilestoneId = 10,
                Title = "Checkpoint Name",
                Description = "Checkpoint Description",
                Complexity = "HIGH",
                StartDate = new DateOnly(2025, 10, 21),
                DueDate = new DateOnly(2025, 10, 26),
                UserId = 8,
                UserRole = RoleConstants.STUDENT
            };

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 11,
                StartDate = new DateOnly(2025, 10, 20),
                EndDate = new DateOnly(2025, 10, 26),
                Team = new Domain.Entities.Team()
                {
                    TeamId = 11,
                    ClassMembers = new List<ClassMember>()
                    {
                        new ClassMember()
                        {
                            ClassMemberId = 3,
                            StudentId = 8
                        }
                    },
                },
            };
            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);
            _checkpointRepoMock.Setup(x => x.Create(It.IsAny<Checkpoint>())).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains("DB Exception", result.Message);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
