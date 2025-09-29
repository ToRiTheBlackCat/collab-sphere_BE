using CollabSphere.Application;
using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using CollabSphere.Application.Features.Subjects.CreateSubject;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;

namespace CollabSphere.Test.SubjectTest
{
    public class CreateSubjectTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateSubjectHandler _handler;

        public CreateSubjectTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new CreateSubjectHandler(_unitOfWorkMock.Object);

        }

        [Fact]
        public async Task HandleCommand_ShouldCreateSubject_WhenValidRequest()
        {
            // Arrange
            var subjectRepo = new Mock<ISubjectRepository>();
            var syllabusRepo = new Mock<ISubjectSyllabusRepository>();
            var outcomeRepo = new Mock<ISubjectOutcomeRepository>();
            var gradeRepo = new Mock<ISubjectGradeComponentRepository>();

            _unitOfWorkMock.Setup(u => u.SubjectRepo).Returns(subjectRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectSyllabusRepo).Returns(syllabusRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectOutcomeRepo).Returns(outcomeRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectGradeComponentRepo).Returns(gradeRepo.Object);

            subjectRepo.Setup(x => x.GetBySubjectCode("CS202")).ReturnsAsync((Subject?)null);
    
            var command = new CreateSubjectCommand
            {
                SubjectName = "Database Systems",
                SubjectCode = "CS202",
                IsActive = true,
                SubjectSyllabus = new SubjectSyllabusDto
                {
                    SyllabusName = "Spring 2026",
                    Description = "Intro to DBs",
                    NoCredit = 3,
                    IsActive = true,
                    SubjectGradeComponents = new List<SubjectGradeComponentDto>
                    {
                        new() { ComponentName = "Exam", ReferencePercentage = 50 },
                        new() { ComponentName = "Project", ReferencePercentage = 50 }
                    },
                    SubjectOutcomes = new List<SubjectOutcomeDto>
                    {
                        new() { OutcomeDetail = "Understand SQL" }
                    }
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal("Subject created successfully.", result.Message);

            _unitOfWorkMock.Verify(u => u.SubjectRepo.Create(It.IsAny<Subject>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SubjectSyllabusRepo.Create(It.IsAny<SubjectSyllabus>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SubjectGradeComponentRepo.Create(It.IsAny<SubjectGradeComponent>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.SubjectOutcomeRepo.Create(It.IsAny<SubjectOutcome>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task ValidateRequest_ShouldFail_WhenGradeComponentsNot100()
        {
            // Arrange
            var subjectRepo = new Mock<ISubjectRepository>();
            var syllabusRepo = new Mock<ISubjectSyllabusRepository>();
            var outcomeRepo = new Mock<ISubjectOutcomeRepository>();
            var gradeRepo = new Mock<ISubjectGradeComponentRepository>();

            _unitOfWorkMock.Setup(u => u.SubjectRepo).Returns(subjectRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectSyllabusRepo).Returns(syllabusRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectOutcomeRepo).Returns(outcomeRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectGradeComponentRepo).Returns(gradeRepo.Object);

            var command = new CreateSubjectCommand
            {
                SubjectName = "Algorithms",
                SubjectCode = "CS301",
                IsActive = true,
                SubjectSyllabus = new SubjectSyllabusDto
                {
                    SyllabusName = "Fall 2026",
                    Description = "Algo Basics",
                    NoCredit = 3,
                    IsActive = true,
                    SubjectGradeComponents = new List<SubjectGradeComponentDto>
                    {
                        new() { ComponentName = "Exam", ReferencePercentage = 70 },
                        new() { ComponentName = "Project", ReferencePercentage = 40 }
                    },
                    SubjectOutcomes = new List<SubjectOutcomeDto>()
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.ErrorList, e => e.Field.Contains("SubjectGradeComponents"));
        }

        [Fact]
            public async Task HandleCommand_ShouldFail_WhenDuplicateSubjectCode()
        {
            // Arrange
            var subjectRepo = new Mock<ISubjectRepository>();
            var syllabusRepo = new Mock<ISubjectSyllabusRepository>();
            var outcomeRepo = new Mock<ISubjectOutcomeRepository>();
            var gradeRepo = new Mock<ISubjectGradeComponentRepository>();

            _unitOfWorkMock.Setup(u => u.SubjectRepo).Returns(subjectRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectSyllabusRepo).Returns(syllabusRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectOutcomeRepo).Returns(outcomeRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectGradeComponentRepo).Returns(gradeRepo.Object);

            var subject = new Subject { SubjectCode = "CS101", SubjectName = "Programming" };
            subjectRepo.Setup(u => u.GetBySubjectCode("CS101")).ReturnsAsync(subject);

            var command = new CreateSubjectCommand
            {
                SubjectName = "Programming Fundamentals",
                SubjectCode = "CS101",
                IsActive = true,
                SubjectSyllabus = new SubjectSyllabusDto
                {
                    SyllabusName = "Fall 2026",
                    IsActive = true,
                    SubjectGradeComponents = new List<SubjectGradeComponentDto>
                    {
                        new() { ComponentName = "Exam", ReferencePercentage = 100 }
                    },
                    SubjectOutcomes = new List<SubjectOutcomeDto>()
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.ErrorList, e => e.Field == "SubjectCode");
        }

        [Fact]
        public async Task HandleCommand_ShouldRollback_WhenExceptionOccurs()
        {
            // Arrange
            var subjectRepo = new Mock<ISubjectRepository>();
            var syllabusRepo = new Mock<ISubjectSyllabusRepository>();
            var outcomeRepo = new Mock<ISubjectOutcomeRepository>();
            var gradeRepo = new Mock<ISubjectGradeComponentRepository>();

            _unitOfWorkMock.Setup(u => u.SubjectRepo).Returns(subjectRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectSyllabusRepo).Returns(syllabusRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectOutcomeRepo).Returns(outcomeRepo.Object);
            _unitOfWorkMock.Setup(u => u.SubjectGradeComponentRepo).Returns(gradeRepo.Object);

            var command = new CreateSubjectCommand
            {
                SubjectName = "Networks",
                SubjectCode = "CS401",
                IsActive = true,
                SubjectSyllabus = new SubjectSyllabusDto
                {
                    SyllabusName = "Fall 2026",
                    IsActive = true,
                    SubjectGradeComponents = new List<SubjectGradeComponentDto>
                    {
                        new() { ComponentName = "Exam", ReferencePercentage = 100 }
                    },
                    SubjectOutcomes = new List<SubjectOutcomeDto>()
                }
            };

            subjectRepo.Setup(u => u.Create(It.IsAny<Subject>()))
                .ThrowsAsync(new Exception("DB insert failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal("DB insert failed", result.Message);

            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }

    }
}
