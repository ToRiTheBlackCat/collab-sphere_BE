using CollabSphere.Application;
using CollabSphere.Application.Constants;
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
        private readonly Mock<ILecturerRepository> _lectureRepoMock;
        private readonly Mock<IClassRepository> _classRepoMock;

        private readonly AssignProjectsToClassHandler _handler;

        public AssignProjectsToClassHandlerTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _assignmentRepoMock = new Mock<IProjectAssignmentRepository>();
            _lectureRepoMock = new Mock<ILecturerRepository>();
            _classRepoMock = new Mock<IClassRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ProjectAssignmentRepo).Returns(_assignmentRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.LecturerRepo).Returns(_lectureRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.ClassRepo).Returns(_classRepoMock.Object);

            _handler = new AssignProjectsToClassHandler(_unitOfWorkMock.Object);
        }

        private static AssignProjectsToClassCommand GenerateValidCommand()
        {
            return new AssignProjectsToClassCommand()
            {
                ClassId = 1,
                ProjectIds = new HashSet<int> { 12, 22 },
                UserId = 2,
                Role = RoleConstants.LECTURER
            };
        }

        [Fact]
        public async Task Handle_ShouldAssignProjecToClass_WhenValidCommand()
        {
            // Arrange
            var command = GenerateValidCommand();

            var existingAssignment = new ProjectAssignment()
            {
                ProjectAssignmentId = 2,
                ProjectId = 22,
                ClassId = 1,
            };
            var removedAssignment = new ProjectAssignment()
            {
                ProjectAssignmentId = 3,
                ProjectId = 13,
                ClassId = 1,
            };

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, LecturerId = 2, ProjectAssignments = { existingAssignment, removedAssignment } });
            _lectureRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Domain.Entities.Lecturer() { LecturerId = 2 });
            _projectRepoMock.Setup(x => x.GetById(12)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 12, Status = 1 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<Domain.Entities.ProjectAssignment>() { existingAssignment, removedAssignment });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _assignmentRepoMock.Verify(x => x.Create(It.Is<ProjectAssignment>(x => x.ProjectId == 12 && x.ClassId == 1)), Times.Once);
            _assignmentRepoMock.Verify(x => x.Delete(removedAssignment), Times.Once);
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
            command.ProjectIds.Add(13);

            var projectAssign1 = new ProjectAssignment()
            {
                ProjectAssignmentId = 1,
                ProjectId = 12,
                ClassId = 1,
            };
            var projectAssign2 = new ProjectAssignment()
            {
                ProjectAssignmentId = 2,
                ProjectId = 22,
                ClassId = 1,
            };

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, LecturerId = 2, ProjectAssignments = { projectAssign1, projectAssign2 } });
            _lectureRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Domain.Entities.Lecturer() { LecturerId = 2 });
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
            command.ProjectIds.Add(13);

            var projectAssign1 = new ProjectAssignment()
            {
                ProjectAssignmentId = 1,
                ProjectId = 12,
                ClassId = 1,
            };
            var projectAssign2 = new ProjectAssignment()
            {
                ProjectAssignmentId = 2,
                ProjectId = 22,
                ClassId = 1,
            };

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, LecturerId = 2, ProjectAssignments = { projectAssign1, projectAssign2 } });
            _projectRepoMock.Setup(x => x.GetById(13)).ReturnsAsync(new Domain.Entities.Project() { ProjectId = 13, Status = 0 });
            _lectureRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Domain.Entities.Lecturer() { LecturerId = 2 });
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
        public async Task Handle_ShouldFailValidation_WhenRemoveProjectAssignedToTeam()
        {
            // Arrange
            var command = GenerateValidCommand();

            var projectAssign1 = new ProjectAssignment()
            {
                ProjectAssignmentId = 1,
                ProjectId = 12,
                ClassId = 1,
            };
            var projectAssign2 = new ProjectAssignment()
            {
                ProjectAssignmentId = 2,
                ProjectId = 22,
                ClassId = 1,
            };
            var removedAssignment = new ProjectAssignment()
            {
                ProjectAssignmentId = 3,
                ProjectId = 13,
                ClassId = 1,
            };
            var team1 = new Domain.Entities.Team()
            {
                TeamId = 1,
                ProjectAssignmentId = 1,
                ProjectAssignment = removedAssignment,
            };

            _classRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, LecturerId = 2, ProjectAssignments = { projectAssign1, projectAssign2, removedAssignment }, Teams = { team1 } });
            _lectureRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(new Domain.Entities.Lecturer() { LecturerId = 2 });
            _assignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<ProjectAssignment>{ projectAssign1, projectAssign2 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Single(result.ErrorList);
            Assert.Contains("Can not remove projects that are assigned", result.ErrorList.First().Message);
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
