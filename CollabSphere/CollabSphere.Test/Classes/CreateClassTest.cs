using CollabSphere.Application;
using CollabSphere.Application.Features.Classes.Commands.CreateClass;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class CreateClassTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ISubjectRepository> _subjectRepo;
        private readonly Mock<IClassRepository> _classRepo;
        private readonly Mock<ILecturerRepository> _lecturerRepo;
        private readonly Mock<IStudentRepository> _studentRepo;
        private readonly Mock<IClassMemberRepository> _classMemberRepo;

        private readonly CreateClassCommandHandler _handler;

        public CreateClassTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _subjectRepo = new Mock<ISubjectRepository>();
            _classRepo = new Mock<IClassRepository>();
            _lecturerRepo = new Mock<ILecturerRepository>();
            _studentRepo = new Mock<IStudentRepository>();
            _classMemberRepo = new Mock<IClassMemberRepository>();

            _unitOfWork.Setup(x => x.SubjectRepo).Returns(_subjectRepo.Object);
            _unitOfWork.Setup(x => x.ClassRepo).Returns(_classRepo.Object);
            _unitOfWork.Setup(x => x.LecturerRepo).Returns(_lecturerRepo.Object);
            _unitOfWork.Setup(x => x.StudentRepo).Returns(_studentRepo.Object);
            _unitOfWork.Setup(x => x.ClassMemberRepo).Returns(_classMemberRepo.Object);

            _handler = new CreateClassCommandHandler(_unitOfWork.Object);
        }

        private CreateClassCommand CreateDefaultRequest()
        {
            return new CreateClassCommand()
            {
                ClassName = "Java Programming Class 2025 Spring",
                EnrolKey = "JV_SP25",
                LecturerId = 1,
                SubjectId = 1,
                StudentIds = new()
                {
                    1, 2
                },
                IsActive = true
            };
        }

        [Fact]
        public async Task HandleCommand_ShouldCreateClass_WhenValidRequest()
        {
            // Arrange
            var request = CreateDefaultRequest();

            var subject = new Subject()
            {
                SubjectId = 1,
                SubjectCode = "VL001",
                SubjectName = "Valid Subject",
                IsActive = true,
            };
            var lecturer = new Lecturer()
            {
                LecturerId = 1,
                Fullname = "Binh Gold",
                Address = "Thanh Hoa",
                LecturerCode = "BG1",
                PhoneNumber = "07715675530",
                AvatarImg = "image_name",
                Yob = 1988,
            };
            var student_1 = new Student()
            {
                StudentId = 1,
                Fullname = "Nguyen Van A",
                AvatarImg = "student_img.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1901",
                Address = "Binh Duong",
            };
            var student_2 = new Student()
            {
                StudentId = 2,
                Fullname = "Nguyen Van B",
                AvatarImg = "student_img2.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1902",
                Address = "Ben Tre",
            };
            _subjectRepo.Setup(x => x.GetById(1)).ReturnsAsync(subject);
            _lecturerRepo.Setup(x => x.GetById(1)).ReturnsAsync(lecturer);
            _studentRepo.Setup(x => x.GetById(1)).ReturnsAsync(student_1);
            _studentRepo.Setup(x => x.GetById(2)).ReturnsAsync(student_2);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Empty(result.ErrorList);
            _unitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _classRepo.Verify(x => x.Create(It.Is<Class>(x => x.ClassName == "Java Programming Class 2025 Spring" && x.SubjectId == 1)), Times.Once);
            _classMemberRepo.Verify(x => x.Create(It.Is<ClassMember>(x => x.StudentId == 1)), Times.Once);
            _classMemberRepo.Verify(x => x.Create(It.Is<ClassMember>(x => x.StudentId == 2)), Times.Once);
            _unitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);    
        }

        [Fact]
        public async Task HandleCommand_ShouldFail_WhenStudentNotExist()
        {
            // Arrange
            var request = CreateDefaultRequest();

            var subject = new Subject()
            {
                SubjectId = 1,
                SubjectCode = "VL001",
                SubjectName = "Valid Subject",
                IsActive = true,
            };
            var lecturer = new Lecturer()
            {
                LecturerId = 1,
                Fullname = "Binh Gold",
                Address = "Thanh Hoa",
                LecturerCode = "BG1",
                PhoneNumber = "07715675530",
                AvatarImg = "image_name",
                Yob = 1988,
            };
            _subjectRepo.Setup(x => x.GetById(1)).ReturnsAsync(subject);
            _lecturerRepo.Setup(x => x.GetById(1)).ReturnsAsync(lecturer);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("No student with ID", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(result.ErrorList, x => x.Message.Contains("No lecturer with ID", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(result.ErrorList, x => x.Message.Contains("No subject with ID", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task HandleCommand_ShouldFail_WhenLecturerNotExist()
        {
            // Arrange
            var request = CreateDefaultRequest();

            var subject = new Subject()
            {
                SubjectId = 1,
                SubjectCode = "VL001",
                SubjectName = "Valid Subject",
                IsActive = true,
            };
            var student_1 = new Student()
            {
                StudentId = 1,
                Fullname = "Nguyen Van A",
                AvatarImg = "student_img.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1901",
                Address = "Binh Duong",
            };
            var student_2 = new Student()
            {
                StudentId = 2,
                Fullname = "Nguyen Van B",
                AvatarImg = "student_img2.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1902",
                Address = "Ben Tre",
            };
            _subjectRepo.Setup(x => x.GetById(1)).ReturnsAsync(subject);
            _studentRepo.Setup(x => x.GetById(1)).ReturnsAsync(student_1);
            _studentRepo.Setup(x => x.GetById(2)).ReturnsAsync(student_2);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("No lecturer with ID", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(result.ErrorList, x => x.Message.Contains("No subject with ID", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(result.ErrorList, x => x.Message.Contains("No Student with ID", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task HandleCommand_ShouldFail_WhenSubjectNotExist()
        {
            // Arrange
            var request = CreateDefaultRequest();

            var lecturer = new Lecturer()
            {
                LecturerId = 1,
                Fullname = "Binh Gold",
                Address = "Thanh Hoa",
                LecturerCode = "BG1",
                PhoneNumber = "07715675530",
                AvatarImg = "image_name",
                Yob = 1988,
            };
            var student_1 = new Student()
            {
                StudentId = 1,
                Fullname = "Nguyen Van A",
                AvatarImg = "student_img.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1901",
                Address = "Binh Duong",
            };
            var student_2 = new Student()
            {
                StudentId = 2,
                Fullname = "Nguyen Van B",
                AvatarImg = "student_img2.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1902",
                Address = "Ben Tre",
            };
            _lecturerRepo.Setup(x => x.GetById(1)).ReturnsAsync(lecturer);
            _studentRepo.Setup(x => x.GetById(1)).ReturnsAsync(student_1);
            _studentRepo.Setup(x => x.GetById(2)).ReturnsAsync(student_2);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.IsValidInput);
            Assert.NotEmpty(result.ErrorList);
            Assert.Contains(result.ErrorList, x => x.Message.Contains("No subject with ID", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(result.ErrorList, x => x.Message.Contains("No lecturer with ID", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(result.ErrorList, x => x.Message.Contains("No Student with ID", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task HandleCommand_ShouldRollbackTransaction_WhenExceptionOccurs()
        {
            var request = CreateDefaultRequest();

            var subject = new Subject()
            {
                SubjectId = 1,
                SubjectCode = "VL001",
                SubjectName = "Valid Subject",
                IsActive = true,
            };
            var lecturer = new Lecturer()
            {
                LecturerId = 1,
                Fullname = "Binh Gold",
                Address = "Thanh Hoa",
                LecturerCode = "BG1",
                PhoneNumber = "07715675530",
                AvatarImg = "image_name",
                Yob = 1988,
            };
            var student_1 = new Student()
            {
                StudentId = 1,
                Fullname = "Nguyen Van A",
                AvatarImg = "student_img.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1901",
                Address = "Binh Duong",
            };
            var student_2 = new Student()
            {
                StudentId = 2,
                Fullname = "Nguyen Van B",
                AvatarImg = "student_img2.png",
                Yob = 2004,
                PhoneNumber = "0664664456",
                StudentCode = "SS1902",
                Address = "Ben Tre",
            };
            _subjectRepo.Setup(x => x.GetById(1)).ReturnsAsync(subject);
            _lecturerRepo.Setup(x => x.GetById(1)).ReturnsAsync(lecturer);
            _studentRepo.Setup(x => x.GetById(1)).ReturnsAsync(student_1);
            _studentRepo.Setup(x => x.GetById(2)).ReturnsAsync(student_2);

            _unitOfWork.Setup(u => u.ClassRepo.Create(It.IsAny<Class>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsValidInput);
            Assert.Contains("DB error", result.Message);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
