using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Checkpoints.Commands.CheckDoneCheckpoint;
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
    public class CheckDoneCheckpointTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICheckpointRepository> _checkpointRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneMock;

        private readonly CheckDoneCheckpointHandler _handler;

        public CheckDoneCheckpointTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _checkpointRepoMock = new Mock<ICheckpointRepository>();
            _teamMilestoneMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.CheckpointRepo).Returns(_checkpointRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneMock.Object);

            _handler = new CheckDoneCheckpointHandler(_unitOfWorkMock.Object);
        }

        private void SetupMocks()
        {
            var checkpoint = new Checkpoint()
            {
                CheckpointId = 15,
                TeamMilestoneId = 10,
                TeamMilestone = new TeamMilestone()
                {
                    TeamMilestoneId = 10,
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
                },
            };

            _checkpointRepoMock.Setup(x => x.GetCheckpointDetail(15)).ReturnsAsync(checkpoint);
            _checkpointRepoMock.Setup(x => x.GetById(15)).ReturnsAsync(checkpoint);
        }

        [Fact]
        public async Task Handle_ShouldCheckDoneCheckpoint_WhenCheckDone()
        {
            // Arrange
            var query = new CheckDoneCheckpointCommand()
            {
                CheckpointId = 15,
                UserId = 11,
                UserRole = RoleConstants.STUDENT
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock.Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Updated status for checkpoint", result.Message);
            Assert.Equal((int)CheckpointStatuses.DONE, capturedCheckpoint.Status);
        }

        [Fact]
        public async Task Handle_ShouldUnCheckDoneCheckpoint_WhenCheckUndone()
        {
            // Arrange
            var query = new CheckDoneCheckpointCommand()
            {
                CheckpointId = 15,
                IsDone = false,
                UserId = 11,
                UserRole = RoleConstants.STUDENT
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock.Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Updated status for checkpoint", result.Message);
            Assert.Equal((int)CheckpointStatuses.NOT_DONE, capturedCheckpoint.Status);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenCheckpointNotFound()
        {
            // Arrange
            var query = new CheckDoneCheckpointCommand()
            {
                CheckpointId = 111,
                IsDone = false,
                UserId = 11,
                UserRole = RoleConstants.STUDENT
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock.Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No checkpoint with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotTeamMember()
        {
            // Arrange
            var query = new CheckDoneCheckpointCommand()
            {
                CheckpointId = 15,
                IsDone = false,
                UserId = 8,
                UserRole = RoleConstants.STUDENT
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock.Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

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
            var query = new CheckDoneCheckpointCommand()
            {
                CheckpointId = 15,
                IsDone = false,
                UserId = 11,
                UserRole = RoleConstants.STUDENT
            };

            this.SetupMocks();

            var capturedCheckpoint = new Checkpoint();
            _checkpointRepoMock.Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Callback<Checkpoint>(x => capturedCheckpoint = x);
            _checkpointRepoMock.Setup(x => x.Update(It.IsAny<Checkpoint>()))
                .Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Empty(result.ErrorList);
            Assert.Contains("DB Exception", result.Message);

            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
