using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Commands.DeleteProject;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Projects
{
    public class DeleteProjectTest
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;

        private readonly DeleteProjectHandler _handler;

        public DeleteProjectTest()
        {

            _uowMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();

            // Link repos to UnitOfWork
            _uowMock.Setup(u => u.ProjectRepo).Returns(_projectRepoMock.Object);

            _handler = new DeleteProjectHandler(_uowMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteProject_WhenValidCommand()
        {
            // Arrange
            var command = new DeleteProjectCommand()
            {
                ProjectId = 12,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, LecturerId = 8, Status = (int)ProjectStatuses.PENDING });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectNotFound()
        {
            // Arrange
            var command = new DeleteProjectCommand()
            {
                ProjectId = 12,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No project with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenIsNotOwningLecturer()
        {
            // Arrange
            var command = new DeleteProjectCommand()
            {
                ProjectId = 12,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, LecturerId = 3, Status = (int)ProjectStatuses.PENDING });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("not the owning Lecturer", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectIsApporved()
        {
            // Arrange
            var command = new DeleteProjectCommand()
            {
                ProjectId = 12,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, LecturerId = 8, Status = (int)ProjectStatuses.APPROVED });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Can not delete a project with APPROVED status", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new DeleteProjectCommand()
            {
                ProjectId = 12,
                UserId = 8,
                UserRole = RoleConstants.LECTURER
            };

            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, LecturerId = 8, Status = (int)ProjectStatuses.PENDING });
            _projectRepoMock.Setup(x => x.Delete(It.IsAny<Project>())).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains("DB Exception", result.Message);

            _projectRepoMock.Verify(x => x.Delete(It.IsAny<Project>()), Times.Once());
            _uowMock.Verify(x => x.RollbackTransactionAsync(), Times.Once());
        }
    }
}
