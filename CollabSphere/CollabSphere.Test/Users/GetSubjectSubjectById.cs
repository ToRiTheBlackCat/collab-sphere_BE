using CollabSphere.Application;
using CollabSphere.Application.Features.Subjects.GetSubjectById;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;

namespace CollabSphere.Test.Users
{
    public class GetSubjectSubjectById
    {

        [Fact]
        public async Task Handle_ShouldReturnSubject_WhenSubjectExists()
        {
            // Arrange
            var subject = new Subject
            {
                SubjectId = 1,
                SubjectName = "Programming",
                SubjectCode = "CS101",
                IsActive = true,
                SubjectSyllabi = new List<SubjectSyllabus>
                {
                    new SubjectSyllabus
                    {
                        SyllabusId = 10,
                        SyllabusName = "Fall 2025",
                        Description = "Intro syllabus",
                        SubjectId = 1,
                        SubjectCode = "CS101",
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true,
                        NoCredit = 3,
                        SubjectGradeComponents = new List<SubjectGradeComponent>
                        {
                            new() { SubjectGradeComponentId = 100, SubjectId = 1, SyllabusId = 10, ComponentName = "Exam", ReferencePercentage = 60 }
                        },
                        SubjectOutcomes = new List<SubjectOutcome>
                        {
                            new() { SubjectOutcomeId = 200, SyllabusId = 10, OutcomeDetail = "Understand basics" }
                        }
                    }
                }
            };

            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetById(1))
                    .ReturnsAsync(subject);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetSubjectByIdQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetSubjectByIdQuery() { SubjectId = 1 }, CancellationToken.None);

            // Assert
            Assert.Empty(result.ErrorList);
            Assert.Empty(result.Message);
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            Assert.NotNull(result.Subject);
            Assert.Equal(1, result.Subject.SubjectId);
            Assert.Equal("Programming", result.Subject.SubjectName);
            Assert.Equal("CS101", result.Subject.SubjectCode);
            Assert.NotNull(result.Subject.SubjectSyllabus);
            Assert.Single(result.Subject.SubjectSyllabus.SubjectGradeComponents);
            Assert.Single(result.Subject.SubjectSyllabus.SubjectOutcomes);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenSubjectDoesNotExist()
        {
            // Arrange
            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetById(99))
                    .ReturnsAsync((Subject?)null);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetSubjectByIdQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetSubjectByIdQuery() { SubjectId = 99 }, CancellationToken.None);

            // Assert
            Assert.Single(result.ErrorList);
            Assert.Empty(result.Message);
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);

            Assert.Null(result.Subject);
        }

        [Fact]
        public async Task Handle_ShouldReturnSubjectWithoutSyllabus_WhenNoSyllabiExist()
        {
            // Arrange
            var subject = new Subject
            {
                SubjectId = 2,
                SubjectName = "Databases",
                SubjectCode = "CS202",
                IsActive = true,
                SubjectSyllabi = new List<SubjectSyllabus>() // empty
            };

            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetById(2))
                    .ReturnsAsync(subject);

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetSubjectByIdQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetSubjectByIdQuery() { SubjectId = 2 }, CancellationToken.None);

            // Assert
            Assert.Empty(result.ErrorList);
            Assert.Empty(result.Message);
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            Assert.NotNull(result.Subject);
            Assert.Null(result.Subject.SubjectSyllabus);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var repoMock = new Mock<ISubjectRepository>();
            repoMock.Setup(r => r.GetById(It.IsAny<int>()))
                    .ThrowsAsync(new Exception("DB error"));

            var unitMock = new Mock<IUnitOfWork>();
            unitMock.Setup(r => r.SubjectRepo).Returns(repoMock.Object);

            var handler = new GetSubjectByIdQueryHandler(unitMock.Object);

            // Act
            var result = await handler.Handle(new GetSubjectByIdQuery() { SubjectId = 1 }, CancellationToken.None);

            // Assert
            Assert.Empty(result.ErrorList);
            Assert.Equal("DB error", result.Message);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);

            Assert.Null(result.Subject);
        }
    }
}
