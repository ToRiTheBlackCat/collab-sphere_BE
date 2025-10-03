using CollabSphere.Application;
using CollabSphere.Application.DTOs.Project;
using CollabSphere.Application.Features.Project.Queries.GetAllProjects;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Projects
{
    public class GetAllProjectTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IDatabase> _redisMock;

        private readonly GetAllProjectsHandler _handler;

        public GetAllProjectTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _redisMock = new Mock<IDatabase>();

            _unitOfWorkMock.Setup(x => x.ProjectRepo).Returns(_projectRepoMock.Object);

            _handler = new GetAllProjectsHandler(_unitOfWorkMock.Object, _redisMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_NoSearchCriteria()
        {
            // Arrange
            var query = new GetAllProjectsQuery()
            {
                Descriptors = string.Empty,
                LecturerIds = new List<int>(),
                SubjectIds = new List<int>()
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Supplies Exchange Web App",
                    Description = "A web app for student to exchange school supplies",
                    LecturerId = 1,
                    SubjectId = 1,
                    Status = 1
                },
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Terminal Calculator",
                    Description = "A CLI based calculator",
                    LecturerId = 1,
                    SubjectId = 2,
                    Status = 1
                }
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal(2, result.Projects.Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_SearchWithSubjectId()
        {
            // Arrange
            var query = new GetAllProjectsQuery()
            {
                Descriptors = string.Empty,
                LecturerIds = new List<int>(),
                SubjectIds = new List<int>() { 2 }
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Supplies Exchange Web App",
                    Description = "A web app for student to exchange school supplies",
                    LecturerId = 1,
                    SubjectId = 1,
                    Status = 1
                },
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Terminal Calculator",
                    Description = "A CLI based calculator",
                    LecturerId = 1,
                    SubjectId = 2,
                    Status = 1
                }
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Single(result.Projects);
            Assert.Equal("Terminal Calculator", result.Projects[0].ProjectName);
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_SearchWithLecturerId()
        {
            // Arrange
            var query = new GetAllProjectsQuery()
            {
                Descriptors = string.Empty,
                LecturerIds = new List<int>() { 1 },
                SubjectIds = new List<int>()
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Supplies Exchange Web App",
                    Description = "A web app for student to exchange school supplies",
                    LecturerId = 1,
                    SubjectId = 1,
                    Status = 1
                },
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Terminal Calculator",
                    Description = "A CLI based calculator",
                    LecturerId = 1,
                    SubjectId = 2,
                    Status = 1
                }
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.NotEmpty(result.Projects);
            Assert.Equal(2, result.Projects.Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnProjects_SearchWithDescriptors()
        {
            // Arrange
            var query = new GetAllProjectsQuery()
            {
                Descriptors = "terminal",
                LecturerIds = new List<int>(),
                SubjectIds = new List<int>()
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Supplies Exchange Web App",
                    Description = "A web app for student to exchange school supplies",
                    LecturerId = 1,
                    SubjectId = 1,
                    Status = 1
                },
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Terminal Calculator",
                    Description = "A CLI based calculator",
                    LecturerId = 1,
                    SubjectId = 2,
                    Status = 1
                }
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Single(result.Projects);
            Assert.Equal("Terminal Calculator", result.Projects[0].ProjectName);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoneMatch()
        {
            // Arrange
            var query = new GetAllProjectsQuery()
            {
                Descriptors = "",
                LecturerIds = new List<int>() { 2 },
                SubjectIds = new List<int>()
            };

            var projects = new List<Project>()
            {
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Supplies Exchange Web App",
                    Description = "A web app for student to exchange school supplies",
                    LecturerId = 1,
                    SubjectId = 1,
                    Status = 1
                },
                new Project()
                {
                    ProjectId = 1,
                    ProjectName = "Terminal Calculator",
                    Description = "A CLI based calculator",
                    LecturerId = 1,
                    SubjectId = 2,
                    Status = 1
                }
            };

            _projectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(projects);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Empty(result.Projects);
        }

        [Fact]
        public async Task Cast_ShouldMapCorrectly_ValidObject()
        {
            // Arrange
            var project = new Project()
            {
                ProjectId = 1,
                ProjectName = "Supplies Exchange Web App",
                Description = "A web app for student to exchange school supplies",
                LecturerId = 1,
                Lecturer = new Lecturer()
                {
                    Fullname = "Lecturer A",
                    LecturerCode = "LT1"
                },
                SubjectId = 1,
                Subject = new Subject()
                {
                    SubjectName = "subject 1",
                    SubjectCode = "SB1"
                },
                Status = 1,
                Objectives = new List<Objective>()
                {
                    new Objective()
                    {
                        Description = "Objective A",
                        Priority = "high",
                        ObjectiveMilestones = new List<ObjectiveMilestone>()
                        {
                            new ObjectiveMilestone()
                            {
                                Title = "Milestone 1",
                                Description = "Milestone Description 1",
                                StartDate = new DateOnly(year: 2012, month: 12, day: 25),
                                EndDate = new DateOnly(year: 2013, month: 1, day: 12),
                            }
                        }
                    }
                },
            };

            // Act
            var projectVM = (ProjectVM)project;

            // Assert
            Assert.Equal(1, projectVM.ProjectId);
            Assert.Equal("Supplies Exchange Web App", projectVM.ProjectName);
            Assert.Equal("A web app for student to exchange school supplies", projectVM.Description);
            Assert.Equal(1, projectVM.LecturerId);
            Assert.Equal("Lecturer A", projectVM.LecturerName);
            Assert.Equal("LT1", projectVM.LecturerCode);
            Assert.Equal(1, projectVM.SubjectId);
            Assert.Equal("subject 1", projectVM.SubjectName);
            Assert.Equal("SB1", projectVM.SubjectCode);
            Assert.Equal(1, projectVM.Status);

            var objective = projectVM.Objectives[0];
            Assert.Equal("Objective A", objective.Description);
            Assert.Equal("high", objective.Priority);

            var milestone = objective.ObjectiveMilestones[0];
            Assert.Equal("Milestone 1", milestone.Title);
            Assert.Equal("Milestone Description 1", milestone.Description);
            Assert.Equal(new DateOnly(year: 2012, month: 12, day: 25), milestone.StartDate);
            Assert.Equal(new DateOnly(year: 2013, month: 1, day: 12), milestone.EndDate);
        }
    }
}
