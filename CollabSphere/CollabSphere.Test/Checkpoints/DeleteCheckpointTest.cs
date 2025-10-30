using Amazon.S3;
using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Checkpoints.Commands.AssignMembersToCheckpoint;
using CollabSphere.Application.Features.Checkpoints.Commands.DeleteCheckpoint;
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
    public class DeleteCheckpointTest
    {
        private readonly Mock<IAmazonS3> _s3ClientMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICheckpointRepository> _checkpointRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneMock;
        private readonly Mock<ICheckpointAssignmentRepository> _assignmentRepoMock;
        private readonly Mock<ICheckpointFileRepository> _fileRepoMock;

        private readonly DeleteCheckpointHandler _handler;

        public DeleteCheckpointTest()
        {
            _s3ClientMock = new Mock<IAmazonS3>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _checkpointRepoMock = new Mock<ICheckpointRepository>();
            _teamMilestoneMock = new Mock<ITeamMilestoneRepository>();
            _assignmentRepoMock = new Mock<ICheckpointAssignmentRepository>();
            _fileRepoMock = new Mock<ICheckpointFileRepository>();

            _unitOfWorkMock.Setup(x => x.CheckpointRepo).Returns(_checkpointRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneMock.Object);
            _unitOfWorkMock.Setup(x => x.CheckpointAssignmentRepo).Returns(_assignmentRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.CheckpointFileRepo).Returns(_fileRepoMock.Object);

            _handler = new DeleteCheckpointHandler(_unitOfWorkMock.Object, _s3ClientMock.Object);
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
                        ClassId = 12,
                        LecturerId = 8,
                        ClassMembers = new List<ClassMember>()
                        {
                            new ClassMember()
                            {
                                ClassMemberId = 11,
                                StudentId = 1,
                            },
                            new ClassMember()
                            {
                                ClassMemberId = 22,
                                StudentId = 2,
                            },
                        },
                    },
                },
            };

            _checkpointRepoMock.Setup(x => x.GetCheckpointDetail(15)).ReturnsAsync(checkpoint);
            _checkpointRepoMock.Setup(x => x.GetById(15)).ReturnsAsync(checkpoint);

            var checkpointAssignments = new List<CheckpointAssignment>()
            {
                new CheckpointAssignment()
                {
                    CheckpointAssignmentId = 1,
                    CheckpointId = 15,
                    ClassMemberId = 22,
                },
            };

            _assignmentRepoMock.Setup(x => x.GetByCheckpointId(15)).ReturnsAsync(checkpointAssignments);

            var checkpointFiles = new List<CheckpointFile>()
            {
                new CheckpointFile()
                {
                    FileId = 1,
                    CheckpointId = 15,
                    FileUrl = Path.Combine("files", "docs", "report1.docx"),
                    Type = "Word Doc",
                },
                new CheckpointFile()
                {
                    FileId = 2,
                    CheckpointId = 15,
                    FileUrl = Path.Combine("files", "docs", "report2.docx"),
                    Type = "Word Doc",
                },
            };

            _fileRepoMock.Setup(x => x.GetFilesByCheckpointId(15)).ReturnsAsync(checkpointFiles);
        }

        [Fact]
        public async Task Handle_ShouldDeleteCheckpoint_WhenValidCommand()
        {
            // Arrange
            var command = new DeleteCheckpointCommand()
            {
                CheckpointId = 15,
                UserId = 1,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Deleted successfully checkpoint", result.Message);

            _fileRepoMock.Verify(
                x => x.Delete(It.Is<CheckpointFile>(x =>
                    x.FileId == 1 || x.FileId == 2
                )),
                Times.Exactly(2)
            );
            _assignmentRepoMock.Verify(
                x => x.Delete(It.Is<CheckpointAssignment>(x =>
                    x.CheckpointId == 15 && x.ClassMemberId == 22
                )),
                Times.Once
            );
            _checkpointRepoMock.Verify(
                x => x.Delete(It.Is<Checkpoint>(x =>
                    x.CheckpointId == 15
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotMemberOfTeam()
        {
            // Arrange
            var command = new DeleteCheckpointCommand()
            {
                CheckpointId = 15,
                UserId = 3,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not a member of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenLecturerOfClass()
        {
            // Arrange
            var command = new DeleteCheckpointCommand()
            {
                CheckpointId = 15,
                UserId = 88,
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
        public async Task Handle_ShouldFailValidation_WhenCheckpointNotFound()
        {
            // Arrange
            var command = new DeleteCheckpointCommand()
            {
                CheckpointId = 155,
                UserId = 1,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No checkpoint with ID: 155", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new DeleteCheckpointCommand()
            {
                CheckpointId = 15,
                UserId = 1,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            _checkpointRepoMock.Setup(x => x.Delete(It.IsAny<Checkpoint>())).Throws(new Exception("DB Exception"));

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
