using CollabSphere.Application;
using CollabSphere.Application.DTOs;
using CollabSphere.Application.Features.User.Queries.GetAllSubject;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Users
{
    public class GetAllSubjectTest
    {
        public GetAllSubjectTest()
        {
            
        }

        [Fact]
        public async Task Handle_ShouldReturnAllSubjects_WhenSubjectsExist()
        {
            // Arrange
            var subjects = new List<Subject>
            {
                new() { SubjectId = 1, SubjectName = "Programming", SubjectCode = "CS101", IsActive = true },
                new() { SubjectId = 2, SubjectName = "Databases", SubjectCode = "CS202", IsActive = false }
            };

            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetAll())
                    .ReturnsAsync(subjects);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetAllSubjectsQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetAllSubjectsQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Subjects.Count);
            Assert.Equal("Programming", result.Subjects[0].SubjectName);
            Assert.Equal("CS202", result.Subjects[1].SubjectCode);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoSubjectsExist()
        {
            // Arrange
            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetAll())
                    .ReturnsAsync(new List<Subject>());

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetAllSubjectsQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetAllSubjectsQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result.Subjects);
            Assert.Empty(result.ErrorList);
            Assert.Empty(result.Message);
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
        }

        [Fact]
        public async Task Handle_ShouldShowInternalError_WhenRepositoryFails()
        {
            // Arrange
            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetAll())
                    .ThrowsAsync(new Exception("Database error"));


            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetAllSubjectsQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetAllSubjectsQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result.Subjects);
            Assert.Empty(result.ErrorList);
            Assert.NotEmpty(result.Message);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
        }

        [Fact]
        public async Task Handle_ShouldMapSubjectsCorrectly()
        {
            // Arrange
            var subjects = new List<Subject>
            {
                new() { SubjectId = 99, SubjectName = "", SubjectCode = "X999", IsActive = true }
            };

            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetAll())
                    .ReturnsAsync(subjects);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetAllSubjectsQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetAllSubjectsQuery(), CancellationToken.None);
            var subject = result.Subjects[0];

            // Assert
            Assert.Single(result.Subjects);
            Assert.Equal(99, subject.SubjectId);
            Assert.Equal("", subject.SubjectName);
            Assert.Equal("X999", subject.SubjectCode);
            Assert.True(subject.IsActive);
        }
    }
}
