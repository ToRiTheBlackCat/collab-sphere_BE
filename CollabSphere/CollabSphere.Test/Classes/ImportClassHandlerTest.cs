using CollabSphere.Application;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.DTOs.Semesters;
using CollabSphere.Application.Features.Classes.Commands.ImportClass;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class ImportClassHandlerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ISubjectRepository> _subjectRepo;
        private readonly Mock<IClassRepository> _classRepo;
        private readonly Mock<ILecturerRepository> _lecturerRepo;
        private readonly Mock<IStudentRepository> _studentRepo;
        private readonly Mock<IClassMemberRepository> _classMemberRepo;
        private readonly Mock<ISemesterRepository> _semesterRepo;

        private readonly ImportClassHandler _handler;
        public ImportClassHandlerTest()
        {

            _unitOfWork = new Mock<IUnitOfWork>();
            _subjectRepo = new Mock<ISubjectRepository>();
            _classRepo = new Mock<IClassRepository>();
            _lecturerRepo = new Mock<ILecturerRepository>();
            _studentRepo = new Mock<IStudentRepository>();
            _classMemberRepo = new Mock<IClassMemberRepository>();
            _semesterRepo = new Mock<ISemesterRepository>();

            _unitOfWork.Setup(x => x.SubjectRepo).Returns(_subjectRepo.Object);
            _unitOfWork.Setup(x => x.ClassRepo).Returns(_classRepo.Object);
            _unitOfWork.Setup(x => x.LecturerRepo).Returns(_lecturerRepo.Object);
            _unitOfWork.Setup(x => x.StudentRepo).Returns(_studentRepo.Object);
            _unitOfWork.Setup(x => x.ClassMemberRepo).Returns(_classMemberRepo.Object);
            _unitOfWork.Setup(x => x.SemesterRepo).Returns(_semesterRepo.Object);

            _handler = new ImportClassHandler(_unitOfWork.Object);
        }

        [Fact]
        public async Task Handle_ValidClassDto_ImportsSuccessfully()
        {
            // Arrange
            var dto = new ImportClassDto
            {
                ClassName = "CS101A",
                EnrolKey = "KEY123",
                SubjectCode = "CS101",
                SemesterCode = "FA25",
                LecturerCode = "LECT001",
                StudentCodes = new List<string> { "STU001", "STU002" },
                IsActive = true
            };

            var subject = new Subject { SubjectCode = "CS101", SubjectId = 1 };
            var lecturer = new Lecturer { LecturerCode = "LECT001", LecturerId = 10 };
            var student1 = new Student { StudentCode = "STU001", StudentId = 100 };
            var student2 = new Student { StudentCode = "STU002", StudentId = 200 };
            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            _subjectRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Subject> { subject });
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>() { lecturer });
            _studentRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Student>() { student1, student2 });
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>() { lecturer });
            _semesterRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Semester>() { semester });

            var command = new ImportClassCommand()
            {
                Classes = new List<ImportClassDto> { dto }
            };

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.True(result.IsSuccess);
            _classRepo.Verify(r => r.Create(It.Is<Class>(x =>
                x.ClassName == dto.ClassName)), Times.Once);
            _classMemberRepo.Verify(r => r.Create(It.Is<ClassMember>(x =>
                x.StudentId == student1.StudentId || x.StudentId == student2.StudentId)), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_SubjectNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new ImportClassDto
            {
                ClassName = "CS101A",
                EnrolKey = "KEY123",
                SubjectCode = "CS404",
                LecturerCode = "LECT001",
                StudentCodes = new List<string>(),
                IsActive = true
            };

            _subjectRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Subject>());
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>());
            _studentRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Student>());
            _semesterRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Semester>());

            var command = new ImportClassCommand()
            {
                Classes = new List<ImportClassDto> { dto }
            };

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("There is no subject with SubjectCode", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Handle_LecturerNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new ImportClassDto
            {
                ClassName = "CS101A",
                EnrolKey = "KEY123",
                SubjectCode = "CS101",
                LecturerCode = "LECT404",
                StudentCodes = new List<string>(),
                IsActive = true
            };


            var subject = new Subject { SubjectCode = "CS101", SubjectId = 1 };
            _subjectRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Subject>() { subject });
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>());
            _semesterRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Semester>());


            var command = new ImportClassCommand()
            {
                Classes = new List<ImportClassDto> { dto }
            };

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("There is no Lecturer with LecturerCode 'LECT404'", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Handle_SomeStudentsNotFound_ReturnsFailure()
        {
            // Arrange
            var dto = new ImportClassDto
            {
                ClassName = "CS101A",
                EnrolKey = "KEY123",
                SubjectCode = "CS101",
                LecturerCode = "LECT001",
                StudentCodes = new List<string> { "STU001", "STU404" },
                IsActive = true
            };

            var subject = new Subject { SubjectCode = "CS101", SubjectId = 1 };
            var lecturer = new Lecturer { LecturerCode = "LECT001", LecturerId = 10 };
            var student1 = new Student { StudentCode = "STU001", StudentId = 100 };

            _subjectRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Subject>());
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>());
            _studentRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Student>());
            _semesterRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Semester>());


            var command = new ImportClassCommand()
            {
                Classes = new List<ImportClassDto> { dto }
            };

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("There were invalid student codes: STU001, STU404", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Handle_ExceptionThrown_RollsBackTransaction()
        {
            // Arrange
            var dto = new ImportClassDto
            {
                ClassName = "CS101A",
                EnrolKey = "KEY123",
                SubjectCode = "CS101",
                SemesterCode = "FA25",
                LecturerCode = "LECT001",
                StudentCodes = new List<string> { "STU001" },
                IsActive = true
            };

            var subject = new Subject { SubjectCode = "CS101", SubjectId = 1 };
            var lecturer = new Lecturer { LecturerCode = "LECT001", LecturerId = 10 };
            var student1 = new Student { StudentCode = "STU001", StudentId = 100 };
            var student2 = new Student { StudentCode = "STU002", StudentId = 200 };
            var semester = new Semester { SemesterId = 1, SemesterName = "Fall 2025", SemesterCode = "FA25", StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 1) };

            _subjectRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Subject> { subject });
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>() { lecturer });
            _studentRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Student>() { student1, student2 });
            _lecturerRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Lecturer>() { lecturer });
            _semesterRepo.Setup(r => r.GetAll())
                .ReturnsAsync(new List<Semester>() { semester });

            _classRepo.Setup(r => r.Create(It.IsAny<Class>()))
                .ThrowsAsync(new Exception("DB Insert failed"));

            var command = new ImportClassCommand()
            {
                Classes = new List<ImportClassDto> { dto }
            };

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("DB Insert failed", result.Message);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
