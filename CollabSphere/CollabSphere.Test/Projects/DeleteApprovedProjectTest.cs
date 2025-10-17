using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Commands.ApproveProject;
using CollabSphere.Application.Features.Project.Commands.DeleteApprovedProject;
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
    public class DeleteApprovedProjectTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IProjectAssignmentRepository> _assignRepoMock;

        private readonly DeleteApprovedProjectHandler _handler;

        public DeleteApprovedProjectTest()
        {
            _projectRepoMock = new Mock<IProjectRepository>();
            _assignRepoMock = new Mock<IProjectAssignmentRepository>();

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ProjectAssignmentRepo).Returns(_assignRepoMock.Object);

            _handler = new DeleteApprovedProjectHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldRemoveProject_WhenValidCommand()
        {
            // Arrange
            var command = new DeleteApprovedProjectCommand()
            {
                ProjectId = 1,
                UserId = 6,
                UserRole = RoleConstants.HEAD_DEPARTMENT,
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 1, Status = (int)ProjectStatuses.APPROVED });
            _assignRepoMock.Setup(x => x.GetProjectAssignmentsByProjectAsync(1)).ReturnsAsync(new List<ProjectAssignment>());

            var capturedProject = new Project();
            _projectRepoMock.Setup(x => x.Update(It.IsAny<Project>())).Callback<Project>(prj => capturedProject = prj);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            Assert.Contains("Deleted project", result.Message);

            Assert.Equal((int)ProjectStatuses.REMOVED, capturedProject.Status);
            Assert.Equal(6, capturedProject.UpdatedBy);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotFoundProject()
        {
            // Arrange
            var command = new DeleteApprovedProjectCommand()
            {
                ProjectId = 1,
                UserId = 6,
                UserRole = RoleConstants.HEAD_DEPARTMENT,
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No project with ID: 1", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectIsNotApproved()
        {
            // Arrange
            var command = new DeleteApprovedProjectCommand()
            {
                ProjectId = 1,
                UserId = 6,
                UserRole = RoleConstants.HEAD_DEPARTMENT,
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 1, Status = (int)ProjectStatuses.DENIED });
            _assignRepoMock.Setup(x => x.GetProjectAssignmentsByProjectAsync(1)).ReturnsAsync(new List<ProjectAssignment>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Can not delete a project that is not APPR", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectIsAssigned()
        {
            // Arrange
            var command = new DeleteApprovedProjectCommand()
            {
                ProjectId = 1,
                UserId = 6,
                UserRole = RoleConstants.HEAD_DEPARTMENT,
            };

            var projectAssignments = new List<ProjectAssignment>()
            {
                new ProjectAssignment()
                {
                    ProjectId = 1,
                    ClassId = 1
                },
                new ProjectAssignment()
                {
                    ProjectId = 1,
                    ClassId = 2
                }
            };
            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 1, Status = (int)ProjectStatuses.APPROVED });
            _assignRepoMock.Setup(x => x.GetProjectAssignmentsByProjectAsync(1)).ReturnsAsync(projectAssignments);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("Can not delete a project that is already", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new DeleteApprovedProjectCommand()
            {
                ProjectId = 1,
                UserId = 6,
                UserRole = RoleConstants.HEAD_DEPARTMENT,
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 1, Status = (int)ProjectStatuses.APPROVED });
            _assignRepoMock.Setup(x => x.GetProjectAssignmentsByProjectAsync(1)).ReturnsAsync(new List<ProjectAssignment>());

            var capturedProject = new Project();
            _projectRepoMock.Setup(x => x.Update(It.IsAny<Project>())).Callback<Project>(prj => capturedProject = prj);
            _projectRepoMock.Setup(x => x.Update(It.IsAny<Project>())).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains("DB Exception", result.Message);
        }
    }
}
