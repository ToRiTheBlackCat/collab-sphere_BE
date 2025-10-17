using CollabSphere.Application;
using CollabSphere.Application.Common;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Student;
using CollabSphere.Application.Features.Student.Queries.GetAllStudent;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CollabSphere.Test.Students
{
    public class GetAllStudentTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IStudentRepository> _studentRepo;
        private readonly Mock<ILogger<GetAllStudentHandler>> _logger;
        private readonly GetAllStudentHandler _handler;

        public GetAllStudentTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _studentRepo = new Mock<IStudentRepository>();
            _logger = new Mock<ILogger<GetAllStudentHandler>>();

            _unitOfWork.Setup(u => u.StudentRepo).Returns(_studentRepo.Object);

            _handler = new GetAllStudentHandler(_unitOfWork.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedList_WhenStudentsFound()
        {
            // Arrange
            var query = new GetAllStudentQuery
            {
                Email = "student1@gmail.com",
                FullName = "Student 1",
                Major = "Software Engineering",
                UserRole = RoleConstants.STAFF
            };

            var students = new List<User>
            {
                new User
                {
                    UId = 1,
                    Email = "student1@gmail.com",
                    Role = new Role { RoleName = "STUDENT" },
                    Student = new Student
                    {
                        Fullname = "Student 1",
                        Address = "HCM",
                        PhoneNumber = "0123456789",
                        Major = "Software Engineering",
                        StudentCode = "SE123",
                        School = "FPT University"
                    },
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            _studentRepo.Setup(r => r.SearchStudent(
                query.Email, query.FullName, query.Yob, query.StudentCode, query.Major, query.IsDesc
            )).ReturnsAsync(students);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.PaginatedStudents);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnMessage_WhenNoStudentsFound()
        {
            // Arrange
            var query = new GetAllStudentQuery
            {
                FullName = "NonExistingStudent",
                UserRole = RoleConstants.STAFF
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _studentRepo.Setup(r => r.SearchStudent(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()
            )).ReturnsAsync(new List<User>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("No lecturer found.", result.Message);
            Assert.Null(result.PaginatedStudents);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            var query = new GetAllStudentQuery
            {
                UserRole = RoleConstants.STAFF
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _studentRepo.Setup(r => r.SearchStudent(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()
            )).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("An error occurred while processing your request.", result.Message);
        }
    }
}
