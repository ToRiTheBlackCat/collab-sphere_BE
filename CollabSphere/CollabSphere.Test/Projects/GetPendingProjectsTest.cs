using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Queries.GetAllProjects;
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
    public class GetPendingProjectsTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;

        private readonly GetPendingProjectsHandler _handler;

        public GetPendingProjectsTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);

            _handler = new GetPendingProjectsHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenProjectFound()
        {
            // Arrange
            var query = new GetPendingProjectsQuery();

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    Status = 2
                },
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.PagedProjects);
            Assert.Equal(2, result.PagedProjects.ItemCount);
            Assert.True(result.PagedProjects.List.All(x => x.Status == ProjectStatuses.PENDING));
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenFilterKeywords()
        {
            // Arrange
            var query = new GetPendingProjectsQuery()
            {
                Descriptors = "Web app",
            };
            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    Status = 2
                },
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.PagedProjects);
            Assert.Equal(1, result.PagedProjects.ItemCount);
            Assert.True(result.PagedProjects.List.All(x => x.Status == ProjectStatuses.PENDING));
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenException()
        {
            // Arrange
            var query = new GetPendingProjectsQuery()
            {
                Descriptors = "Web app"
            };

            _projectRepoMock.Setup(x => x.GetAll()).ThrowsAsync(new Exception("DB Exception."));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.PagedProjects);
            Assert.Contains("DB Exception", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
