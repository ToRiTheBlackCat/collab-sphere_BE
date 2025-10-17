using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Features.Lecturer.Queries.GetAllLec;
using CollabSphere.Application.Mappings.Lecturer;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CollabSphere.Test.Lecturers
{
    public class GetAllLecturerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILecturerRepository> _mockLecturerRepo;
        private readonly Mock<ILogger<GetAllLecturerHandler>> _logger;
        private readonly GetAllLecturerHandler _handler;

        public GetAllLecturerTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockLecturerRepo = new Mock<ILecturerRepository>();
            _logger = new Mock<ILogger<GetAllLecturerHandler>>();

            _unitOfWork.Setup(u => u.LecturerRepo).Returns(_mockLecturerRepo.Object);
            _handler = new GetAllLecturerHandler(_unitOfWork.Object, _logger.Object);
        }

        [Fact]
        public async Task HandleCommand_ShouldReturnPaginatedLecturerList_WhenLecturersFound()
        {
            // Arrange
            var query = new GetAllLecturerQuery
            {
                Email = "lec1@fpt.edu.vn",
                FullName = "Lecturer1",
                Major = "Software Engineering",
                UserRole = RoleConstants.STAFF
            };

            var lecturerList = new List<User>
            {
                new User
                {
                    UId = 1,
                    Email = "lec1@fpt.edu.vn",
                    IsTeacher = true,
                    RoleId = RoleConstants.LECTURER,
                    Role = new Role { RoleName = "LECTURER" },
                    Lecturer = new CollabSphere.Domain.Entities.Lecturer
                    {
                        Fullname = "Lecturer1",
                        Major = "Software Engineering"
                    },
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockLecturerRepo
                .Setup(r => r.SearchLecturer(
                    query.Email, query.FullName, query.Yob, query.LecturerCode, query.Major, query.IsDesc))
                .ReturnsAsync(lecturerList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.PaginatedLecturers);
            Assert.Single(result.PaginatedLecturers.List);
            Assert.Equal("Lecturer1", result.PaginatedLecturers.List.First().Fullname);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleCommand_ShouldReturnMessage_WhenNoLecturerFound()
        {
            // Arrange
            var query = new GetAllLecturerQuery
            {
                FullName = "NotExist",
                UserRole = RoleConstants.STAFF
            };

            _mockLecturerRepo.Setup(r => r.SearchLecturer(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<User>());

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("No lecturer found.", result.Message);
            Assert.Null(result.PaginatedLecturers);
        }

        [Fact]
        public async Task HandleCommand_ShouldRollbackTransaction_WhenExceptionThrown()
        {
            // Arrange
            var query = new GetAllLecturerQuery
            {
                UserRole = RoleConstants.STAFF
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            _mockLecturerRepo.Setup(r => r.SearchLecturer(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
