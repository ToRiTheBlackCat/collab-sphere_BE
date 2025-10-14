using CollabSphere.Application;
using CollabSphere.Application.Features.ProjectAssignments.Commands.AssignProjectsToClass;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.ProjectAssignments
{
    public class AssignProjectsToClassHandlerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IProjectAssignmentRepository> _assignmentRepoMock;
        private readonly Mock<IClassRepository> _classRepoMock;

        private readonly AssignProjectsToClassHandler _handler;

        public AssignProjectsToClassHandlerTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _assignmentRepoMock = new Mock<IProjectAssignmentRepository>();
            _classRepoMock = new Mock<IClassRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ProjectAssignmentRepo).Returns(_assignmentRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ClassRepo).Returns(_classRepoMock.Object);

            _handler = new AssignProjectsToClassHandler(_unitOfWorkMock.Object);
        }

        private static AssignProjectsToClassCommand GenerateValidCommand()
        {
            return new AssignProjectsToClassCommand()
            {
                ClassId = 1,
                ProjectIds = new HashSet<int> { 12, 22 }
            };
        }

        [Fact]
        public async Task Handle_ShouldAssignProjecToClass_WhenValidCommand()
        {
            // Arrange
            var command = GenerateValidCommand();

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1 });
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            _projectRepoMock.Setup(x => x.GetById(22)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 22, Status = 1 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<Domain.Entities.ProjectAssignment>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _assignmentRepoMock.Verify(x => x.Create(It.Is<ProjectAssignment>(x => x.ProjectId == 12 && x.ClassId == 1)), Times.Once);
            _assignmentRepoMock.Verify(x => x.Create(It.Is<ProjectAssignment>(x => x.ProjectId == 22 && x.ClassId == 1)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenClassNotExist()
        {
            // Arrange
            var command = GenerateValidCommand();

            _classRepoMock.Setup(x => x.GetById(1));
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            _projectRepoMock.Setup(x => x.GetById(22)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 22, Status = 1 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<Domain.Entities.ProjectAssignment>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains("No Class with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectNotExist()
        {
            // Arrange
            var command = GenerateValidCommand();

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1 });
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            //_projectRepoMock.Setup(x => x.GetById(22)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 22, Status = 1 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<Domain.Entities.ProjectAssignment>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains("No Project with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectNotApproved()
        {
            // Arrange
            var command = GenerateValidCommand();

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1 });
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            _projectRepoMock.Setup(x => x.GetById(22)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 22, Status = 0 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<Domain.Entities.ProjectAssignment>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains("is not APPROVED", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenProjectAlreadyAssigned()
        {
            // Arrange
            var command = GenerateValidCommand();

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1 });
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            _projectRepoMock.Setup(x => x.GetById(22)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 22, Status = 1 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1))
                .ReturnsAsync(new List<Domain.Entities.ProjectAssignment>() { new ProjectAssignment() { ClassId = 1, ProjectId = 22 } });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains("is already assigned Project with ID", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBackTransaction_WhenException()
        {
            // Arrange
            var command = GenerateValidCommand();

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1 });
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            _projectRepoMock.Setup(x => x.GetById(22)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 22, Status = 1 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1))
                .ReturnsAsync(new List<Domain.Entities.ProjectAssignment>() { new ProjectAssignment() { ClassId = 1, ProjectId = 22 } });

            _classRepoMock.Setup(x => x.GetById(1)).ThrowsAsync(new Exception("DB Exception"));


            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Empty(result.ErrorList);
            Assert.Equal("DB Exception", result.Message);
        }
    }
}
