using CollabSphere.Application;
using CollabSphere.Application.DTOs.Lecturer;
using CollabSphere.Application.Features.Admin.Queries;
using CollabSphere.Application.Features.Lecturer.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Lecturers
{
    public class GetAllLecturerTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILecturerRepository> _mockLecturerRepo;
        private readonly Mock<ILogger<GetAllLecturerHandler>> _mockLogger;
        private readonly GetAllLecturerHandler _handler;

        public GetAllLecturerTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLecturerRepo = new Mock<ILecturerRepository>();
            _mockLogger = new Mock<ILogger<GetAllLecturerHandler>>();
            _mockUnitOfWork.Setup(c => c.LecturerRepo).Returns(_mockLecturerRepo.Object);
            _handler = new GetAllLecturerHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllLecturerHandler_ShouldReturnMappedList_WhenLecturerListFound()
        {
            //Arrange
            var dto = new GetAllLecturerRequestDto
            {
                Email = "sampleLec1@gmail.com",
                FullName = "Lecturer1",
                Major = "Software Engineering"
            };
            var request = new GetAllLecturerCommand(dto);

            var lecturerList = new List<User>
            {
                new User
                {
                    UId = 1,
                    Email = "sampleLec1@gmail.com",
                    IsTeacher = true,
                    RoleId = 4,
                    Role = new Role { RoleName = "LECTURER" },
                    Lecturer = new Domain.Entities.Lecturer
                    {
                        Fullname = "Lecturer1",
                        Address = "HCM",
                        PhoneNumber = "0123456789",
                        Yob = 1990,
                        AvatarImg = "avatar_123",
                        School = "FPT University",
                        LecturerCode = "LE123123",
                        Major = "Software Engineering"
                    },
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _mockLecturerRepo.Setup(repo => repo.SearchLecturer(
                dto.Email, dto.FullName, dto.Yob, dto.LecturerCode,
                dto.Major, dto.PageNumber, dto.PageSize, dto.IsDesc
            )).ReturnsAsync(lecturerList);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemCount);
            Assert.Equal("Lecturer1", result.LecturerList[0].Fullname);
            Assert.Equal("LECTURER", result.LecturerList[0].RoleName);
        }

        [Fact]
        public async Task GetAllLecturerHandler_ShouldReturnEmptyList_WhenLecturerListNotFound()
        {
            // Arrange
            var dto = new GetAllLecturerRequestDto { FullName = "NotExitedLecturer" };
            var request = new GetAllLecturerCommand(dto);

            _mockLecturerRepo.Setup(repo => repo.SearchLecturer(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()
            )).ReturnsAsync(new List<User>());

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result.LecturerList);
            Assert.Equal(0, result.ItemCount);
        }

        [Fact]
        public async Task GetAllLecturerHandler_ShouldReturnNull_WhenExceptionThrown()
        {
            // Arrange
            var dto = new GetAllLecturerRequestDto();
            var request = new GetAllLecturerCommand(dto);

            _mockLecturerRepo.Setup(repo => repo.SearchLecturer(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<bool>()
            )).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
        }
    }
}
