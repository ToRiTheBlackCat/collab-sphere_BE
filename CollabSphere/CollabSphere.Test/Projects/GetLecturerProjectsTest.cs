using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Queries.GetTeacherProjects;
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
    public class GetLecturerProjectsTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        //private readonly Mock<ILecturerRepository> _lecturerRepoMock;
        private readonly GetLecturerProjectsHandler _handler;

        public GetLecturerProjectsTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            //_lecturerRepoMock = new Mock<ILecturerRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);
            //_unitOfWorkMock.Setup(x => x.LecturerRepo).Returns(_lecturerRepoMock.Object);

            _handler = new GetLecturerProjectsHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenNoFilter()
        {
            // Arrange
            var query = new GetLecturerProjectsQuery()
            {
                LecturerId = 1,
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Project 1",
                    LecturerId = 1,
                    Status = 0
                },                                    
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "Project 2",
                    LecturerId = 1,
                    Status = 1
                },                                  
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Project 3",
                    LecturerId = 1,
                    Status = 2
                },
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Projects.Count);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenFindByDescriptors()
        {
            // Arrange
            var query = new GetLecturerProjectsQuery()
            {
                LecturerId = 1,
                Descriptors = "platform app"
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = 1
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = 2
                },
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result.Projects);
            Assert.Equal(2, result.Projects.Count);
            Assert.DoesNotContain(result.Projects, x => {
                var hasName = x.ProjectName.Contains("platform") || x.ProjectName.Contains("app");
                var hasDescription = x.Description.Contains("platform") || x.Description.Contains("app");

                return !hasName && !hasDescription;
            });
        }


        [Fact]
        public async Task Handle_ShouldReturnProjects_WhenFindByStatus()
        {
            // Arrange
            var query = new GetLecturerProjectsQuery()
            {
                LecturerId = 1,
                Statuses = new HashSet<int>()
                {
                    ProjectStatuses.PENDING,
                }
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = 2
                },
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(result.Projects);
            Assert.Equal(2, result.Projects.Count);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenFilterNotMath()
        {
            // Arrange
            var query = new GetLecturerProjectsQuery()
            {
                LecturerId = 2,
                Statuses = new HashSet<int>()
                {
                    ProjectStatuses.PENDING,
                }
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = 0
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = 2
                },
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Empty(result.Projects);
        }

        [Fact]
        public async Task Handle_ReturnError_WhenDBException()
        {
            // Arrange
            var query = new GetLecturerProjectsQuery()
            {
                LecturerId = 2,
                Statuses = new HashSet<int>()
                {
                    ProjectStatuses.PENDING,
                }
            };

            _projectRepoMock.Setup(x => x.GetAll()).ThrowsAsync(new Exception("DB Exception."));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal("DB Exception.", result.Message);
        }
    }
}
