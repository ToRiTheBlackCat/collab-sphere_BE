using CollabSphere.Application;
using CollabSphere.Application.DTOs.SubjectGradeComponentModels;
using CollabSphere.Application.DTOs.SubjectOutcomeModels;
using CollabSphere.Application.DTOs.SubjectSyllabusModel;
using CollabSphere.Application.Features.Subjects.Commands.UpdateSubject;
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
    public class UpdateSubjectTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ISubjectRepository> _subjectRepo;
        private readonly Mock<ISubjectSyllabusRepository> _syllabusRepo;
        private readonly Mock<ISubjectOutcomeRepository> _outcomeRepo;
        private readonly Mock<ISubjectGradeComponentRepository> _gradeComponentRepo;

        private readonly UpdateSubjectHandler _handler;
        public UpdateSubjectTest()
        {
            _subjectRepo = new Mock<ISubjectRepository>();
            _syllabusRepo = new Mock<ISubjectSyllabusRepository>();
            _outcomeRepo = new Mock<ISubjectOutcomeRepository>();
            _gradeComponentRepo = new Mock<ISubjectGradeComponentRepository>();

            _unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWork.Setup(x => x.SubjectRepo).Returns(_subjectRepo.Object);
            _unitOfWork.Setup(x => x.SubjectSyllabusRepo).Returns(_syllabusRepo.Object);
            _unitOfWork.Setup(x => x.SubjectOutcomeRepo).Returns(_outcomeRepo.Object);
            _unitOfWork.Setup(x => x.SubjectGradeComponentRepo).Returns(_gradeComponentRepo.Object);

            _handler = new UpdateSubjectHandler(_unitOfWork.Object);
        }

        private UpdateSubjectCommand CreateValidRequest()
        {
            return new UpdateSubjectCommand
            {
                SubjectId = 1,
                SubjectName = "Advanced Database Systems",
                SubjectCode = "DBS201",
                IsActive = true,
                SubjectSyllabus = new SubjectSyllabusDto
                {
                    SyllabusName = "2025 Edition",
                    Description = "Covers advanced topics in database design and performance.",
                    NoCredit = 3,
                    IsActive = true,
                    SubjectGradeComponents = new List<SubjectGradeComponentDto>
                    {
                        new() { ComponentName = "Midterm Exam", ReferencePercentage = 40 },
                        new() { ComponentName = "Final Exam", ReferencePercentage = 60 }
                    },
                    SubjectOutcomes = new List<SubjectOutcomeDto>
                    {
                        new() { OutcomeDetail = "Students can design normalized schemas" },
                        new() { OutcomeDetail = "Students understand query optimization" }
                    }
                }
            };
        }


        [Fact]
        public async Task HandleCommand_ShouldUpdateWithCorrectValues()
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

            _subjectRepo.Setup(x => x.GetById(subject.SubjectId)).ReturnsAsync(subject);
            _gradeComponentRepo.Setup(x => x.GetAll()).ReturnsAsync(new List<SubjectGradeComponent>());
            _outcomeRepo.Setup(x => x.GetAll()).ReturnsAsync(new List<SubjectOutcome>());

            var request = new UpdateSubjectCommand
            {
                SubjectId = 1,
                SubjectName = "Updated Programming",
                SubjectCode = "CS101A",
                IsActive = true,
                SubjectSyllabus = new SubjectSyllabusDto
                {
                    SyllabusName = "Updated Syllabus",
                    Description = "Updated description",
                    NoCredit = 3,
                    IsActive = true,
                    SubjectGradeComponents = new List<SubjectGradeComponentDto>
                    {
                        new SubjectGradeComponentDto { ComponentName = "Project", ReferencePercentage = 40 },
                        new SubjectGradeComponentDto { ComponentName = "Exam", ReferencePercentage = 60 }
                    },
                    SubjectOutcomes = new List<SubjectOutcomeDto>
                    {
                        new SubjectOutcomeDto { OutcomeDetail = "Updated Outcome" }
                    }
                }
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _subjectRepo.Verify(r => r.Update(It.Is<Subject>(s =>
                s.SubjectId == request.SubjectId &&
                s.SubjectName == request.SubjectName &&
                s.SubjectCode == request.SubjectCode &&
                s.IsActive == request.IsActive
            )), Times.Once);
            _syllabusRepo.Verify(r => r.Update(It.IsAny<SubjectSyllabus>()), Times.Once);
            _gradeComponentRepo.Verify(r => r.Create(It.IsAny<SubjectGradeComponent>()), Times.Exactly(2));
            _outcomeRepo.Verify(r => r.Create(It.IsAny<SubjectOutcome>()), Times.Exactly(1));
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);

        }

        [Fact]
        public async Task Handler_ShouldFail_WhenGradeComponentsNotSumTo100()
        {
            // Arrange
            var request = CreateValidRequest();
            request.SubjectSyllabus.SubjectGradeComponents = new()
            {
                new() { ComponentName = "Midterm", ReferencePercentage = 40 },
                new() { ComponentName = "Final", ReferencePercentage = 40 }
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("Sum up to 100", StringComparison.CurrentCultureIgnoreCase));
            _subjectRepo.Verify(r => r.Update(It.IsAny<Subject>()), Times.Never);
        }

        [Fact]
        public async Task Handler_ShouldFail_WhenDuplicateSubjectCode()
        {
            // Arrange
            var request = CreateValidRequest();
            var otherSubject = new Subject { SubjectId = 2, SubjectCode = "DBS201" };
            _subjectRepo.Setup(r => r.GetBySubjectCode("DBS201")).ReturnsAsync(otherSubject);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Contains(result.ErrorList, x => 
                x.Field == nameof(request.SubjectCode) && 
                x.Message.Contains("already exist", StringComparison.OrdinalIgnoreCase)
            );
            _unitOfWork.Verify(r => r.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleCommand_ShouldRollback_WhenRepositoryFail()
        {
            // Arrange
            var request = CreateValidRequest();
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

            _subjectRepo.Setup(x => x.GetById(1)).ReturnsAsync(subject);
            _unitOfWork.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB Exception."));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Equal("DB Exception.", result.Message);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
