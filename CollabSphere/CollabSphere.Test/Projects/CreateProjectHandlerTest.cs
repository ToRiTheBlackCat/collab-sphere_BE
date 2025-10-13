using CollabSphere.Application;
using CollabSphere.Application.DTOs.Objective;
using CollabSphere.Application.DTOs.ObjectiveMilestone;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Project.Commands.CreateProject;
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
    public class CreateProjectHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<ILecturerRepository> _lecturerRepoMock;
        private readonly Mock<ISubjectRepository> _subjectRepoMock;
        private readonly CreateProjectHandler _handler;

        public CreateProjectHandlerTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _lecturerRepoMock = new Mock<ILecturerRepository>();
            _subjectRepoMock = new Mock<ISubjectRepository>();

            // Link repos to UnitOfWork
            _uowMock.Setup(u => u.ProjectRepo).Returns(_projectRepoMock.Object);
            _uowMock.Setup(u => u.LecturerRepo).Returns(_lecturerRepoMock.Object);
            _uowMock.Setup(u => u.SubjectRepo).Returns(_subjectRepoMock.Object);

            _handler = new CreateProjectHandler(_uowMock.Object);
        }

        public static CreateProjectDTO CreateValidProjectDTO()
        {
            return new CreateProjectDTO
            {
                ProjectName = "AI Learning Assistant",
                Description = "Project using AI to support students.",
                LecturerId = 1,
                SubjectId = 1,
                Objectives = new List<CreateProjectObjectiveDTO>
                {
                    new CreateProjectObjectiveDTO
                    {
                        Description = "Phase 1",
                        Priority = "High",
                        ObjectiveMilestones = new List<CreateProjectObjectiveMilestoneDTO>
                        {
                            new CreateProjectObjectiveMilestoneDTO
                            {
                                Title = "Milestone 1",
                                Description = "Initial planning",
                                StartDate = new DateOnly(2025,10,13),
                                EndDate = new DateOnly(2025,10,16)
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task HandleCommand_ShouldCreateProject_WhenRequestIsValid()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            var command = new CreateProjectCommand
            {
                UserId = dto.LecturerId,
                Project = dto
            };

            _lecturerRepoMock.Setup(r => r.GetById(dto.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = dto.LecturerId });
            _subjectRepoMock.Setup(r => r.GetById(dto.SubjectId))
                .ReturnsAsync(new Subject { SubjectId = dto.SubjectId });

            _projectRepoMock.Setup(r => r.Create(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Project Created Successfully.", result.Message);
            _uowMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _uowMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task ValidateRequest_ShouldFail_WhenUserIdDoesNotMatchLecturerId()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            var command = new CreateProjectCommand
            {
                UserId = 99, // mismatch
                Project = dto
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains("doesn't match the Project's LecturerId", result.ErrorList[0].Message);
        }

        [Fact]
        public async Task ValidateRequest_ShouldFail_WhenLecturerNotFound()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            var command = new CreateProjectCommand
            {
                UserId = dto.LecturerId,
                Project = dto
            };

            _lecturerRepoMock.Setup(r => r.GetById(dto.LecturerId))
                .ReturnsAsync((Lecturer?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains("No existing Lecturer", result.ErrorList[0].Message);
        }

        [Fact]
        public async Task ValidateRequest_ShouldFail_WhenSubjectNotFound()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            var command = new CreateProjectCommand { UserId = dto.LecturerId, Project = dto };

            _lecturerRepoMock.Setup(r => r.GetById(dto.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = dto.LecturerId });
            _subjectRepoMock.Setup(r => r.GetById(dto.SubjectId))
                .ReturnsAsync((Subject?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("No existing Subject", result.ErrorList[0].Message);
        }

        [Fact]
        public async Task ValidateRequest_ShouldFail_WhenMilestoneEndIsTooClose()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            dto.Objectives[0].ObjectiveMilestones[0].StartDate = new DateOnly(2025, 10, 10);
            dto.Objectives[0].ObjectiveMilestones[0].EndDate = new DateOnly(2025, 10, 11); // <2 days apart

            var command = new CreateProjectCommand
            {
                UserId = dto.LecturerId,
                Project = dto
            };


            _lecturerRepoMock.Setup(r => r.GetById(dto.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = dto.LecturerId });
            _subjectRepoMock.Setup(r => r.GetById(dto.SubjectId))
                .ReturnsAsync(new Subject { SubjectId = dto.SubjectId });

            _projectRepoMock.Setup(r => r.Create(It.IsAny<Project>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains("atleast 2 days after", result.ErrorList[0].Message);
        }

        [Fact]
        public async Task HandleCommand_ShouldRollback_WhenExceptionThrown()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            var command = new CreateProjectCommand { UserId = dto.LecturerId, Project = dto };

            _lecturerRepoMock.Setup(r => r.GetById(dto.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = dto.LecturerId });
            _subjectRepoMock.Setup(r => r.GetById(dto.SubjectId))
                .ReturnsAsync(new Subject { SubjectId = dto.SubjectId });

            _projectRepoMock.Setup(r => r.Create(It.IsAny<Project>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("DB error", result.Message);
            _uowMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleCommand_ShouldMapFieldsCorrectly_WhenCreatingProject()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            var command = new CreateProjectCommand { UserId = dto.LecturerId, Project = dto };
            Project? createdProject = null;

            _lecturerRepoMock.Setup(r => r.GetById(dto.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = dto.LecturerId });
            _subjectRepoMock.Setup(r => r.GetById(dto.SubjectId))
                .ReturnsAsync(new Subject { SubjectId = dto.SubjectId });
            _projectRepoMock.Setup(r => r.Create(It.IsAny<Project>()))
                .Callback<Project>(p => createdProject = p)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(createdProject);
            Assert.Equal(dto.ProjectName, createdProject.ProjectName);
            Assert.Equal(dto.LecturerId, createdProject.LecturerId);
            Assert.Equal(dto.SubjectId, createdProject.SubjectId);
            Assert.Single(createdProject.Objectives);
        }

        [Fact]
        public async Task ValidateRequest_ShouldFail_WhenRequiredFieldsMissing()
        {
            // Arrange
            var dto = CreateValidProjectDTO();
            dto.ProjectName = ""; // violates [Required] + [Length]

            var command = new CreateProjectCommand { UserId = dto.LecturerId, Project = dto };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("ProjectName", result.ErrorList[0].Field);
        }
    }
}
