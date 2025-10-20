using CollabSphere.Application;
using CollabSphere.Application.Features.Team.Commands;
using CollabSphere.Application.Features.Team.Commands.UpdateTeam;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Team
{
    public class UpdateTeamTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<IStudentRepository> _studentRepoMock;
        private readonly Mock<ILecturerRepository> _lecturerRepoMock;
        private readonly Mock<IProjectAssignmentRepository> _projectAssignmentRepoMock;
        private readonly Mock<ILogger<UpdateTeamHandler>> _loggerMock;

        private readonly UpdateTeamHandler _handler;

        public UpdateTeamTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _classRepoMock = new Mock<IClassRepository>();
            _studentRepoMock = new Mock<IStudentRepository>();
            _lecturerRepoMock = new Mock<ILecturerRepository>();
            _projectAssignmentRepoMock = new Mock<IProjectAssignmentRepository>();
            _loggerMock = new Mock<ILogger<UpdateTeamHandler>>();

            _unitOfWorkMock.SetupGet(u => u.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.ClassRepo).Returns(_classRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.StudentRepo).Returns(_studentRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.LecturerRepo).Returns(_lecturerRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.ProjectAssignmentRepo).Returns(_projectAssignmentRepoMock.Object);

            _handler = new UpdateTeamHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task UpdateTeam_ShouldSucceed_WhenAllValid()
        {
            // Arrange
            var command = new UpdateTeamCommand
            {
                TeamId = 1,
                TeamName = "Updated Team",
                EnrolKey = "ABC123",
                Description = "Updated description",
                GitLink = "https://github.com/team",
            };

            var foundTeam = new Domain.Entities.Team
            {
                TeamId = 1,
                TeamName = "Old Team",
                EnrolKey = "OLDKEY",
                Description = "Old description",
                GitLink = "old link",
                ClassId = 2,
                CreatedDate = new DateOnly(2025, 1, 1)
            };

            _teamRepoMock.Setup(r => r.GetById(command.TeamId)).ReturnsAsync(foundTeam);

            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("updated successfully", result.Message);
            _teamRepoMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Team>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTeam_ShouldFail_WhenTeamNotFound()
        {
            // Arrange
            var command = new UpdateTeamCommand
            {
                TeamId = 99,
                TeamName = "NonExistent",
            };

            _teamRepoMock.Setup(r => r.GetById(command.TeamId)).ReturnsAsync((Domain.Entities.Team?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            _teamRepoMock.Verify(r => r.Update(It.IsAny<Domain.Entities.Team>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTeam_ShouldRollback_WhenExceptionThrown()
        {
            // Arrange
            var command = new UpdateTeamCommand
            {
                TeamId = 1,
                TeamName = "TeamCrash",
            };

            var foundTeam = new Domain.Entities.Team { TeamId = 1, TeamName = "Old Team" };

            _teamRepoMock.Setup(r => r.GetById(command.TeamId)).ReturnsAsync(foundTeam);
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _teamRepoMock.Setup(r => r.Update(It.IsAny<Domain.Entities.Team>()))
            .Callback(() => throw new Exception(""));

            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
        }
    }
}
