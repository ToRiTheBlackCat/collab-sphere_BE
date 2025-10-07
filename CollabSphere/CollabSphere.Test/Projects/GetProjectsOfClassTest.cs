using CollabSphere.Application;
using CollabSphere.Application.Features.Project.Queries.GetProjectsOfClass;
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
    public class GetProjectsOfClassTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectAssignmentRepository> _projectAssignmentRepoMock;

        private readonly GetProjectsOfClassHandler _handler;

        public GetProjectsOfClassTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectAssignmentRepoMock = new Mock<IProjectAssignmentRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectAssignmentRepo).Returns(_projectAssignmentRepoMock.Object);

            _handler = new GetProjectsOfClassHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenFoundProjects()
        {
            // Arrange
            var query = new GetProjectsOfClassQuery()
            {
                ClassId = 1,
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Calculator App",
                    Description = "A terminal cli-based calculator app."
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "Exchange Platform",
                    Description = "A public web platform for user to exchange items with each other."
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Collab Group",
                    Description = "A web system for students to collab with each other in private groups."
                },
            };

            var projectAssignments = new List<ProjectAssignment>()
            {
                new ProjectAssignment()
                {
                    ProjectAssignmentId = 1,
                    ClassId = 1,
                    ProjectId = 1,
                    Project = projects[0],
                    Status = 1,
                },
                new ProjectAssignment()
                {
                    ProjectAssignmentId = 2,
                    ClassId = 1,
                    ProjectId = 2,
                    Project = projects[1],
                    Status = 1,
                },
                new ProjectAssignment()
                {
                    ProjectAssignmentId = 3,
                    ClassId = 2,
                    ProjectId = 3,
                    Project = projects[2],
                    Status = 1,
                },
            };

            _projectAssignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<ProjectAssignment> { projectAssignments[0], projectAssignments[1] });
            _projectAssignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(2)).ReturnsAsync(new List<ProjectAssignment> { projectAssignments[2] });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PagedProjects);
            Assert.NotEmpty(result.PagedProjects.List);
            Assert.Equal(2, result.PagedProjects.ItemCount);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenFindByKeywords()
        {
            // Arrange
            var query = new GetProjectsOfClassQuery()
            {
                ClassId = 1,
                Descriptors = "WEB"
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Calculator App",
                    Description = "A terminal cli-based calculator app."
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "Exchange Platform",
                    Description = "A public web platform for user to exchange items with each other."
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Collab Group",
                    Description = "A web system for students to collab with each other in private groups."
                },
            };

            var projectAssignments = new List<ProjectAssignment>()
            {
                new ProjectAssignment()
                {
                    ProjectAssignmentId = 1,
                    ClassId = 1,
                    ProjectId = 1,
                    Project = projects[0],
                    Status = 1,
                },
                new ProjectAssignment()
                {
                    ProjectAssignmentId = 2,
                    ClassId = 1,
                    ProjectId = 2,
                    Project = projects[1],
                    Status = 1,
                },
                new ProjectAssignment()
                {
                    ProjectAssignmentId = 3,
                    ClassId = 2,
                    ProjectId = 3,
                    Project = projects[2],
                    Status = 1,
                },
            };

            _projectAssignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(1)).ReturnsAsync(new List<ProjectAssignment> { projectAssignments[0], projectAssignments[1] });
            _projectAssignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(2)).ReturnsAsync(new List<ProjectAssignment> { projectAssignments[2] });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PagedProjects);
            Assert.NotEmpty(result.PagedProjects.List);
            Assert.Equal(1, result.PagedProjects.ItemCount);
            Assert.True(result.PagedProjects.List.All(x => $"{x.ProjectName} | {x.Description}".Contains("web", StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoProjectMatch()
        {
            // Arrange
            var query = new GetProjectsOfClassQuery()
            {
                ClassId = 2,
                Descriptors = "WEB"
            };

            _projectAssignmentRepoMock.Setup(x => x.GetProjectAssignmentsByClassAsync(It.IsAny<int>())).ReturnsAsync(new List<ProjectAssignment>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result.PagedProjects);
            Assert.Empty(result.PagedProjects.List);
            Assert.Equal(0, result.PagedProjects.ItemCount);
        }
    }
}
