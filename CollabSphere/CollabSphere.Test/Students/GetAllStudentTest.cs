using CollabSphere.Application;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Application.Features.Student.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Students
{
    public class GetAllStudentTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<ILogger<GetAllStudentHandler>> _mockLogger;
        private readonly GetAllStudentHandler _handler;

        public GetAllStudentTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockLogger = new Mock<ILogger<GetAllStudentHandler>>();
            _mockUnitOfWork.Setup(c => c.StudentRepo).Returns(_mockStudentRepo.Object);
            _handler = new GetAllStudentHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllStudentHandler_ShouldReturnMappedList_WhenStudentListFound()
        {
            //Arrange
            var dto = new GetAllStudentRequestDto
            {
                Email = "sampleStu1@gmail.com",
                FullName = "Student1",
                Major = "Software Engineering"
            };
            var request = new GetAllStudentCommand(dto);

            var studentList = new List<User>
            {
                new User
                {
                    UId = 1,
                    Email = "sampleStu1@gmail.com",
                    IsTeacher = true,
                    RoleId = 5,
                    Role = new Role { RoleName = "STUDENT" },
                    Student = new Domain.Entities.Student
                    {
                        Fullname = "Student1",
                        Address = "HCM",
                        PhoneNumber = "0123456789",
                        Yob = 1990,
                        AvatarImg = "avatar_123",
                        School = "FPT University",
                        StudentCode = "ST123123",
                        Major = "Software Engineering"
                    },
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _mockStudentRepo.Setup(repo => repo.SearchStudent(
                dto.Email, dto.FullName, dto.Yob, dto.StudentCode,
                dto.Major, dto.PageNumber, dto.PageSize, dto.IsDesc
            )).ReturnsAsync(studentList);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemCount);
            Assert.Equal("Student1", result.StudentList[0].Fullname);
            Assert.Equal("STUDENT", result.StudentList[0].RoleName);
        }

        [Fact]
        public async Task GetAllStudentHandler_ShouldReturnEmptyList_WhenStudentListNotFound()
        {
            // Arrange
            var dto = new GetAllStudentRequestDto { FullName = "NotExitedLecturer" };
            var request = new GetAllStudentCommand(dto);

            _mockStudentRepo.Setup(repo => repo.SearchStudent(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()
            )).ReturnsAsync(new List<User>());

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Empty(result.StudentList);
            Assert.Equal(0, result.ItemCount);
        }

        [Fact]
        public async Task GetAllStudentHandler_ShouldReturnNull_WhenExceptionThrown()
        {
            // Arrange
            var dto = new GetAllStudentRequestDto();
            var request = new GetAllStudentCommand(dto);

            _mockStudentRepo.Setup(repo => repo.SearchStudent(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()
            )).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(null, result);
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
