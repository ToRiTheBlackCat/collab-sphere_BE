using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Classes.Commands.AssignLec;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Classes
{
    public class AssignLecturerToClassTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<ILecturerRepository> _lecturerRepoMock;
        private readonly Mock<ILogger<AssignLecturerToClassHandler>> _loggerMock;
        private readonly AssignLecturerToClassHandler _handler;

        public AssignLecturerToClassTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _classRepoMock = new Mock<IClassRepository>();
            _lecturerRepoMock = new Mock<ILecturerRepository>();
            _loggerMock = new Mock<ILogger<AssignLecturerToClassHandler>>();

            _unitOfWorkMock.SetupGet(u => u.ClassRepo).Returns(_classRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.LecturerRepo).Returns(_lecturerRepoMock.Object);

            _handler = new AssignLecturerToClassHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AssignLecturerToClassHandler_ShouldAssignLecturer_WhenClassAndLecturerExist()
        {
            // Arrange
            var command = new AssignLecturerToClassCommand
            {
                ClassId = 1,
                LecturerId = 2,
                UserId = 10,
                UserRole = RoleConstants.STAFF
            };

            var mockClass = new Class { ClassId = 1, LecturerId = null };
            var mockLecturer = new Lecturer { LecturerId = 2 };

            _classRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(mockClass);
            _lecturerRepoMock.Setup(r => r.GetById(2)).ReturnsAsync(mockLecturer);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Lecturer assigned to class successfully.", result.Message);
            _classRepoMock.Verify(r => r.Update(mockClass), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignLecturerToClassHandler_ShouldReturnErrorMessage_WhenExceptionThrown()
        {
            // Arrange
            var command = new AssignLecturerToClassCommand
            {
                ClassId = 1,
                LecturerId = 2,
                UserId = 10,
                UserRole = RoleConstants.STAFF
            };

            _classRepoMock.Setup(r => r.GetById(It.IsAny<int>())).ThrowsAsync(new Exception("An error occurred while processing your request."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("An error occurred while processing your request.", result.Message);
        }
    }
}
