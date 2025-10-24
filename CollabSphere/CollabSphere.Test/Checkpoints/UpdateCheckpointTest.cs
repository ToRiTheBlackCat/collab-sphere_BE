using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Checkpoints;
using CollabSphere.Application.Features.Checkpoints.Commands.CheckDoneCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Commands.UpdateCheckpoint;
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
    public class UpdateCheckpointTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICheckpointRepository> _checkpointRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneMock;

        private readonly UpdateCheckpointHandler _handler;

        public UpdateCheckpointTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _checkpointRepoMock = new Mock<ICheckpointRepository>();
            _teamMilestoneMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.CheckpointRepo).Returns(_checkpointRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneMock.Object);

            _handler = new UpdateCheckpointHandler(_unitOfWorkMock.Object);
        }

        private void SetupMocks()
        {
            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
            };

            _checkpointRepoMock.Setup(x => x.GetById(15)).ReturnsAsync(checkpoint);

            var milestone = new TeamMilestone()
            {
                TeamMilestoneId = 10,
                TeamId = 7,
                Team = new Domain.Entities.Team()
                {
                    TeamId = 7,
                    ClassId = 1,
                    LecturerId = 8,
                    ClassMembers = new List<ClassMember>()
                        {
                            new ClassMember()
                            {
                                ClassMemberId = 11,
                                StudentId = 11,
                            },
                        },
                },
                StartDate = new DateOnly(2025, 4, 1),
                EndDate = new DateOnly(2025, 8, 30)
            };

            _teamMilestoneMock.Setup(x => x.GetById(10)).ReturnsAsync(milestone);
        }

        [Fact]
        public async Task Handle_ShouldUpdateCheckpoint_WhenValidCommand()
        {
            // Arrange
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 4, 12),
                DueDate = new DateOnly(2025, 8, 23),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Updated checkpoint", result.Message);

            Assert.Equal(15, capturedCheckpoint.CheckpointId);
            Assert.Equal(10, capturedCheckpoint.TeamMilestoneId);
            Assert.Equal("Title", capturedCheckpoint.Title);
            Assert.Equal("Description", capturedCheckpoint.Description);
            Assert.Equal("High impact", capturedCheckpoint.Complexity);
            Assert.Equal(new DateOnly(2025, 4, 12), capturedCheckpoint.StartDate);
            Assert.Equal(new DateOnly(2025, 8, 23), capturedCheckpoint.DueDate);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenCheckpointNotFound()
        {
            // Arrange
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 111,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 4, 12),
                DueDate = new DateOnly(2025, 8, 23),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No checkpoint with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenMilestoneNotFound()
        {
            // Arrange
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 111,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 4, 12),
                DueDate = new DateOnly(2025, 8, 23),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No team milestone with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotMemberOfTeam()
        {
            // Arrange
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 4, 12),
                DueDate = new DateOnly(2025, 8, 23),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 222,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

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
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 8, 23),
                DueDate = new DateOnly(2025, 4, 12),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("StartDate can't be a date before DueDate", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenStartDateNotInMilestoneDateRange()
        {
            // Arrange
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 3, 12),
                DueDate = new DateOnly(2025, 8, 23),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("StartDate can't be a date before milestone's StartDate", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenEndDateNotInMilestoneDateRange()
        {
            // Arrange
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 4, 12),
                DueDate = new DateOnly(2025, 9, 1),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

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
            var updateDto = new UpdateCheckpointDto()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                Title = "Title",
                Description = "Description",
                Complexity = "High impact",
                StartDate = new DateOnly(2025, 4, 12),
                DueDate = new DateOnly(2025, 8, 24),
            };

            var command = new UpdateCheckpointCommand()
            {
                UpdateDto = updateDto,
                UserId = 11,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            _checkpointRepoMock
                .Setup(x => x.Update(It.IsAny<Checkpoint>())).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Empty(result.ErrorList);
            Assert.Contains("DB Exception", result.Message);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
