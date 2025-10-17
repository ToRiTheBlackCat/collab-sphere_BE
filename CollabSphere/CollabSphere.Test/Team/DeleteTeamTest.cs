using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Team.Commands;
using CollabSphere.Application.Features.Team.Commands.DeleteTeam;
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
    public class DeleteTeamTest
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ITeamRepository> _mockTeamRepo;
        private readonly Mock<ILecturerRepository> _mockLecturerRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<ILogger<DeleteTeamHandler>> _mockLogger;

        public DeleteTeamTest()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockTeamRepo = new Mock<ITeamRepository>();
            _mockLecturerRepo = new Mock<ILecturerRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockLogger = new Mock<ILogger<DeleteTeamHandler>>();

            _mockUow.Setup(u => u.TeamRepo).Returns(_mockTeamRepo.Object);
            _mockUow.Setup(u => u.LecturerRepo).Returns(_mockLecturerRepo.Object);
            _mockUow.Setup(u => u.StudentRepo).Returns(_mockStudentRepo.Object);
        }

        [Fact]
        public async Task DeleteTeamHandler_ShouldDeleteTeam_WhenTeamExists()
        {
            // Arrange
            var team = new CollabSphere.Domain.Entities.Team { TeamId = 1, Status = 1 };
            _mockTeamRepo.Setup(r => r.GetById(1)).ReturnsAsync(team);

            var handler = new DeleteTeamHandler(_mockUow.Object, _mockLogger.Object);
            var command = new DeleteTeamCommand { TeamId = 1, UserId = 99, UserRole = RoleConstants.ADMIN };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team has been successfully deleted.", result.Message);
            _mockTeamRepo.Verify(r => r.Update(It.Is<Domain.Entities.Team>(t => t.Status == 0)), Times.Once);
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTeamHandler_ShouldFail_WhenTeamNotFound()
        {
            // Arrange
            _mockTeamRepo.Setup(r => r.GetById(99)).ReturnsAsync((Domain.Entities.Team?)null);

            var handler = new DeleteTeamHandler(_mockUow.Object, _mockLogger.Object);
            var command = new DeleteTeamCommand { TeamId = 99, UserId = 1, UserRole = RoleConstants.STAFF };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            _mockTeamRepo.Verify(r => r.Update(It.IsAny<Domain.Entities.Team>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTeamHandler_ShouldRollback_WhenExceptionThrown()
        {
            // Arrange
            var team = new Domain.Entities.Team { TeamId = 5, Status = 1 };
            _mockTeamRepo.Setup(r => r.GetById(5)).ReturnsAsync(team);
            _mockUow.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB Error"));

            var handler = new DeleteTeamHandler(_mockUow.Object, _mockLogger.Object);
            var command = new DeleteTeamCommand { TeamId = 5, UserId = 2, UserRole = RoleConstants.ADMIN };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("An error occurred while processing your request.", result.Message);
            _mockUow.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
