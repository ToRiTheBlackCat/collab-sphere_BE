using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Project.Queries.GetProjectById;
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
    public class GetProjectByIdTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<ILecturerRepository> _lecturerRepoMock;

        private readonly GetProjectByIdHandler _handler;
        public GetProjectByIdTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _lecturerRepoMock = new Mock<ILecturerRepository>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.LecturerRepo).Returns(_lecturerRepoMock.Object);

            _handler = new GetProjectByIdHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProject_WhenValidRequest()
        {
            // Arrange
            var query = new GetProjectByIdQuery()
            {
                ProjectId = 2,
                UserId = 1,
                UserRole = RoleConstants.STUDENT
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = ProjectStatuses.PENDING,
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = ProjectStatuses.APPROVED
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = ProjectStatuses.DENIED
                },
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(projects[0]);
            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(projects[1]);
            _projectRepoMock.Setup(x => x.GetById(3)).ReturnsAsync(projects[2]);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsAuthorized);
            Assert.NotNull(result.Project);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenProjectNotFound()
        {
            // Arrange
            var query = new GetProjectByIdQuery()
            {
                ProjectId = 4,
                UserId = 1,
                UserRole = RoleConstants.STUDENT
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = ProjectStatuses.PENDING,
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = ProjectStatuses.APPROVED
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = ProjectStatuses.DENIED
                },
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(projects[0]);
            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(projects[1]);
            _projectRepoMock.Setup(x => x.GetById(3)).ReturnsAsync(projects[2]);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Project);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotAuthorized_WhenNotAuthorized()
        {
            // Arrange
            var query = new GetProjectByIdQuery()
            {
                ProjectId = 1,
                UserId = 2,
                UserRole = RoleConstants.STUDENT
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = ProjectStatuses.PENDING,
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = ProjectStatuses.APPROVED
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = ProjectStatuses.DENIED
                },
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(projects[0]);
            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(projects[1]);
            _projectRepoMock.Setup(x => x.GetById(3)).ReturnsAsync(projects[2]);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsAuthorized);
            Assert.NotNull(result.Project);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotAuthorized_WhenNotOwningLecturer()
        {
            // Arrange
            var query = new GetProjectByIdQuery()
            {
                ProjectId = 1,
                UserId = 2,
                UserRole = RoleConstants.LECTURER
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = ProjectStatuses.PENDING,
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = ProjectStatuses.APPROVED
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = ProjectStatuses.DENIED
                },
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(projects[0]);
            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(projects[1]);
            _projectRepoMock.Setup(x => x.GetById(3)).ReturnsAsync(projects[2]);

            var lecturer = new Lecturer()
            {
                LecturerId = 2,
                Fullname = "Nguyen Van A",
            };

            _lecturerRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(lecturer);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsAuthorized);
            Assert.NotNull(result.Project);
        }

        [Fact]
        public async Task Handle_ShouldReturnProject_WhenIsOwningLecturer()
        {
            // Arrange
            var query = new GetProjectByIdQuery()
            {
                ProjectId = 1,
                UserId = 1,
                UserRole = RoleConstants.LECTURER
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = ProjectStatuses.PENDING,
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = ProjectStatuses.APPROVED
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = ProjectStatuses.DENIED
                },
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(projects[0]);
            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(projects[1]);
            _projectRepoMock.Setup(x => x.GetById(3)).ReturnsAsync(projects[2]);

            var lecturer = new Lecturer()
            {
                LecturerId = 1,
                Fullname = "Nguyen Van A",
            };

            _lecturerRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(lecturer);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsAuthorized);
            Assert.NotNull(result.Project);
        }

        [Fact]
        public async Task Handle_ShouldReturnProject_WhenIsBypassRole()
        {
            // Arrange
            var query = new GetProjectByIdQuery()
            {
                ProjectId = 1,
                UserId = 3,
                UserRole = RoleConstants.ADMIN // By pass view privileges Role
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Exchange app",
                    Description = "A platform for users to post, exchange items with each other.",
                    LecturerId = 1,
                    Status = ProjectStatuses.PENDING,
                },
                new Project()
                {
                    ProjectId = 2,
                    ProjectName = "CLI Calculator",
                    Description = "Terminal interface for calculating basic operations.",
                    LecturerId = 1,
                    Status = ProjectStatuses.APPROVED
                },
                new Project()
                {
                    ProjectId = 3,
                    ProjectName = "Media Web app",
                    Description = "A web platform for student to share pictures, videos, ...",
                    LecturerId = 1,
                    Status = ProjectStatuses.DENIED
                },
            };

            _projectRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(projects[0]);
            _projectRepoMock.Setup(x => x.GetById(2)).ReturnsAsync(projects[1]);
            _projectRepoMock.Setup(x => x.GetById(3)).ReturnsAsync(projects[2]);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsAuthorized);
            Assert.NotNull(result.Project);
        }
    }
}
