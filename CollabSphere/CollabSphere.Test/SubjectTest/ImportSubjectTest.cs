using CollabSphere.Application;
using CollabSphere.Application.DTOs.SubjectModels;
using CollabSphere.Application.Features.Subjects.Commands.ImportSubject;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.SubjectTest
{
    public class ImportSubjectTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ISubjectRepository> _subjectRepoMock;
        private readonly Mock<ISubjectSyllabusRepository> _syllabusRepoMock;
        private readonly Mock<ISubjectGradeComponentRepository> _subjectGradeRepoMock;
        private readonly Mock<ISubjectOutcomeRepository> _subjectOutcomeRepoMock;

        private readonly ImportSubjectHandler _handler;
        public ImportSubjectTest()
        {
            _subjectRepoMock = new Mock<ISubjectRepository>();
            _syllabusRepoMock = new Mock<ISubjectSyllabusRepository>();
            _subjectGradeRepoMock = new Mock<ISubjectGradeComponentRepository>();
            _subjectOutcomeRepoMock = new Mock<ISubjectOutcomeRepository>();

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.SubjectRepo).Returns(_subjectRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.SubjectSyllabusRepo).Returns(_syllabusRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.SubjectGradeComponentRepo).Returns(_subjectGradeRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.SubjectOutcomeRepo).Returns(_subjectOutcomeRepoMock.Object);

            _handler = new ImportSubjectHandler(_unitOfWorkMock.Object);
        }

        private ImportSubjectCommand CreateValidCommand()
        {
            return new ImportSubjectCommand()
            {
                Subjects = new List<ImportSubjectDto>
                {
                    new ImportSubjectDto()
                    {
                        SubjectCode = "EW101",
                        SubjectName = "Academic Writing in University",
                        IsActive = true,
                    },
                    new ImportSubjectDto()
                    {
                        SubjectCode = "CS101",
                        SubjectName = "Introduction to Object-Oriented Programming",
                        IsActive = true,
                    }
                }
            };
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenNoSyllabus()
        {
            // Arrange
            var command = CreateValidCommand();

            _subjectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(new List<Domain.Entities.Subject>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenValidSyllabus()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Subjects[1].SubjectSyllabus = new Application.DTOs.SubjectSyllabusModel.ImportSubjectSyllabusDto()
            {
                Description = "Syllabus Description",
                SyllabusName = "Syllabus for subject CS101",
                NoCredit = 1,
                SubjectGradeComponents = new List<Application.DTOs.SubjectGradeComponentModels.ImportSubjectGradeComponentDto>()
                {
                    new Application.DTOs.SubjectGradeComponentModels.ImportSubjectGradeComponentDto()
                    {
                        ComponentName = "Grade Comp 1",
                        ReferencePercentage = 65,
                    },
                    new Application.DTOs.SubjectGradeComponentModels.ImportSubjectGradeComponentDto()
                    {
                        ComponentName = "Grade Comp 2",
                        ReferencePercentage = 35,
                    }
                },
                SubjectOutcomes = new List<Application.DTOs.SubjectOutcomeModels.ImportSubjectOutcomeDto>()
                {
                    new Application.DTOs.SubjectOutcomeModels.ImportSubjectOutcomeDto()
                    {
                        OutcomeDetail = "Create Product"
                    },
                    new Application.DTOs.SubjectOutcomeModels.ImportSubjectOutcomeDto()
                    {
                        OutcomeDetail = "Learn concepts"
                    },
                }
            };

            _subjectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(new List<Domain.Entities.Subject>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);

            _syllabusRepoMock.Verify(
                x => x.Create(It.Is<SubjectSyllabus>(x => 
                        x.SubjectCode == "CS101" &&
                        x.Description == "Syllabus Description" &&
                        x.SyllabusName == "Syllabus for subject CS101" &&
                        x.NoCredit == 1
                    )),
                Times.Once
            );
            _subjectGradeRepoMock.Verify(
                x => x.Create(It.Is<SubjectGradeComponent>(x =>
                        (x.ComponentName == "Grade Comp 1" && x.ReferencePercentage == 65) ||
                        (x.ComponentName == "Grade Comp 2" && x.ReferencePercentage == 35)
                    )),
                Times.Exactly(2)
            );
            _subjectOutcomeRepoMock.Verify(
                x => x.Create(It.Is<SubjectOutcome>(x =>
                        (x.OutcomeDetail == "Create Product" && x.Syllabus.SyllabusName == "Syllabus for subject CS101") ||
                        (x.OutcomeDetail == "Learn concepts" && x.Syllabus.SyllabusName == "Syllabus for subject CS101")
                    )),
                Times.Exactly(2)
            );


            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }


        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenSyllabusHasNoOutcomes()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Subjects[1].SubjectSyllabus = new Application.DTOs.SubjectSyllabusModel.ImportSubjectSyllabusDto()
            {
                Description = "Syllabus Description",
                SyllabusName = "Syllabus for subject CS101",
                NoCredit = 1,
            };

            _subjectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(new List<Domain.Entities.Subject>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains(result.ErrorList, x => x.Field.Contains("SubjectGradeComponents") && x.Message == "Can't be an empty sequence.");
            Assert.Contains(result.ErrorList, x => x.Field.Contains("SubjectOutcomes") && x.Message == "Can't be an empty sequence.");
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenNoSubjectsProvided()
        {
            // Arrange
            var command = new ImportSubjectCommand()
            {
                Subjects = new List<ImportSubjectDto>(),
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);

            Assert.Contains(result.ErrorList, x => x.Field.Equals("Subjects") && x.Message.Contains("There are no subjects to be imported"));
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenDuplcatedSubjectCode()
        {
            // Arrange
            var command = new ImportSubjectCommand()
            {
                Subjects = new List<ImportSubjectDto>
                {
                    new ImportSubjectDto()
                    {
                        SubjectCode = "EW101",
                        SubjectName = "Academic Writing in University",
                        IsActive = true,
                    },
                    new ImportSubjectDto()
                    {
                        SubjectCode = "EW101",
                        SubjectName = "Introduction to Object-Oriented Programming",
                        IsActive = true,
                    }
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);

            Assert.Contains(result.ErrorList, x => x.Field.Equals("Subjects") && x.Message.Contains("Duplicated SubjectCodes found"));
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenSubjectAlreadyExist()
        {
            // Arrange
            var command = CreateValidCommand();

            var subjects = new List<Subject>()
            {
                new Subject()
                {
                    SubjectId = 1,
                    SubjectCode = "CS101",
                },
            };
            _subjectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(subjects);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);

            Assert.Contains(result.ErrorList, x => x.Message.Contains("Subject with SubjectCode 'CS101' already exist"));
        }

        [Fact]
        public async Task Handle_ShouldRollBackTransaction_WhenDBException()
        {
            // Arrange
            var command = CreateValidCommand();

            _subjectRepoMock.Setup(x => x.GetAll()).ReturnsAsync(new List<Domain.Entities.Subject>());
            _unitOfWorkMock.Setup(x => x.CommitTransactionAsync()).ThrowsAsync(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal("DB Exception", result.Message);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }
}
