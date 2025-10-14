using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Commands.ApproveProject;
using CollabSphere.Application.Features.Project.Queries.GetPendingProjects;
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
    public class ApproveProjectTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;

        private readonly ApproveProjectHandler _handler;

        public ApproveProjectTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);

            _handler = new ApproveProjectHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldApproveProject_WhenValidRequest()
        {
            // Arrange
            var command = new ApproveProjectCommand()
            {
                ProjectId = 1
            };

            var project = new Project()
            {
                ProjectId = 1,
                ProjectName = "Project Name 1",
                Description = "Description for Project 1",
                Status = (int)ProjectStatuses.PENDING
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(project);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            _unitOfWorkMock.Verify(x => x.ProjectRepo.Update(It.Is<Project>(x => x.ProjectName.Equals("Project Name 1") && x.Status == (int)ProjectStatuses.APPROVED)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
            Assert.Contains("Project Name 1", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Handle_ShouldDenyProject_WhenValidRequest()
        {
            // Arrange
            var command = new ApproveProjectCommand()
            {
                ProjectId = 1,
                Approve = false
            };

            var project = new Project()
            {
                ProjectId = 1,
                ProjectName = "Project Name 1",
                Description = "Description for Project 1",
                Status = (int)ProjectStatuses.PENDING
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(project);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            _unitOfWorkMock.Verify(x => x.ProjectRepo.Update(It.Is<Project>(x => x.ProjectName.Equals("Project Name 1") && x.Status == (int)ProjectStatuses.DENIED)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
            Assert.Contains("Project Name 1", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Handle_ShouldReturnRequestError_WhenProjectNotFound()
        {
            // Arrange
            var command = new ApproveProjectCommand()
            {
                ProjectId = 1
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("No Project with ID"));
        }

        [Fact]
        public async Task Handle_ShouldReturnRequestError_WhenProjectIsNotPending()
        {
            // Arrange
            var command = new ApproveProjectCommand()
            {
                ProjectId = 1
            };

            var project = new Project()
            {
                ProjectId = 1,
                ProjectName = "Project Name 1",
                Description = "Description for Project 1",
                Status = (int)ProjectStatuses.APPROVED
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(project);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("not PENDING"));
        }


        [Fact]
        public async Task Handle_ShouldRollBackTransaction_WhenExceptionOccur()
        {
            // Arrange
            var command = new ApproveProjectCommand()
            {
                ProjectId = 1
            };

            var project = new Project()
            {
                ProjectId = 1,
                ProjectName = "Project Name 1",
                Description = "Description for Project 1",
                Status = (int)ProjectStatuses.APPROVED
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ThrowsAsync(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal("DB Exception", result.Message);
        }
    }
}
