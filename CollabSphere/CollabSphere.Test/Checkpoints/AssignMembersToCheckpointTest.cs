using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Checkpoints.Commands.AssignMembersToCheckpoint;
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
    public class AssignMembersToCheckpointTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICheckpointRepository> _checkpointRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneMock;
        private readonly Mock<ICheckpointAssignmentRepository> _assignmentRepoMock;

        private readonly AssignMembersToCheckpointHandler _handler;

        public AssignMembersToCheckpointTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _checkpointRepoMock = new Mock<ICheckpointRepository>();
            _teamMilestoneMock = new Mock<ITeamMilestoneRepository>();
            _assignmentRepoMock = new Mock<ICheckpointAssignmentRepository>();

            _unitOfWorkMock.Setup(x => x.CheckpointRepo).Returns(_checkpointRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneMock.Object);
            _unitOfWorkMock.Setup(x => x.CheckpointAssignmentRepo).Returns(_assignmentRepoMock.Object);

            _handler = new AssignMembersToCheckpointHandler(_unitOfWorkMock.Object);
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
        }

        [Fact]
        public async Task Handle_ShouldAssignMembers_WhenValidCommand()
        {
            // Arrange
            var command = new AssignMembersToCheckpointCommand()
            {
                AssignmentsDto = new Application.DTOs.CheckpointAssignments.CheckpointAssignmentsDto()
                {
                    CheckpointId = 15,
                    ClassMemberIds = { 11 },
                },
                UserId = 1,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Updated member assignments for checkpoint", result.Message);

            _assignmentRepoMock.Verify(
                x => x.Delete(It.Is<CheckpointAssignment>(x =>
                    x.CheckpointAssignmentId == 1
                )),
                Times.Once
            );
            _assignmentRepoMock.Verify(
                x => x.Create(It.Is<CheckpointAssignment>(x =>
                    x.CheckpointId == 15 &&
                    x.ClassMemberId == 11
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenCheckpointNotFound()
        {
            // Arrange
            var command = new AssignMembersToCheckpointCommand()
            {
                AssignmentsDto = new Application.DTOs.CheckpointAssignments.CheckpointAssignmentsDto()
                {
                    CheckpointId = 111,
                    ClassMemberIds = { 11 },
                },
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
            Assert.Contains("No checkpoint with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotMemberOfTeam()
        {
            // Arrange
            var command = new AssignMembersToCheckpointCommand()
            {
                AssignmentsDto = new Application.DTOs.CheckpointAssignments.CheckpointAssignmentsDto()
                {
                    CheckpointId = 15,
                    ClassMemberIds = { 11 },
                },
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
        public async Task Handle_ShouldFailValidation_WhenInvalidMembers()
        {
            // Arrange
            var command = new AssignMembersToCheckpointCommand()
            {
                AssignmentsDto = new Application.DTOs.CheckpointAssignments.CheckpointAssignmentsDto()
                {
                    CheckpointId = 15,
                    ClassMemberIds = { 33, 11, 44 },
                },
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
            Assert.Contains("33, 44", result.ErrorList.First().Message);
            Assert.Contains("are not members of the team", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new AssignMembersToCheckpointCommand()
            {
                AssignmentsDto = new Application.DTOs.CheckpointAssignments.CheckpointAssignmentsDto()
                {
                    CheckpointId = 15,
                    ClassMemberIds = { 22, 11 },
                },
                UserId = 1,
                UserRole = RoleConstants.STUDENT,
            };

            this.SetupMocks();

            _assignmentRepoMock.Setup(x => x.Create(It.IsAny<CheckpointAssignment>())).Throws(new Exception("DB Exception"));

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
