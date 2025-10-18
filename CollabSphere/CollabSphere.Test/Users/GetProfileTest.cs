using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.DTOs.Validation;
using CollabSphere.Application.Features.Lecturer.Queries.GetAllLec;
using CollabSphere.Application.Features.User.Queries.GetUserById;
using CollabSphere.Application.Mappings.User;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using CollabSphere.Infrastructure.Base;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Users
{
    public class GetProfileTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly GetUserProfileByIdHandler _handler;

        public GetProfileTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();

            _mockUnitOfWork.Setup(u => u.UserRepo).Returns(_mockUserRepo.Object);
            _handler = new GetUserProfileByIdHandler(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetUserProfileByIdHandler_ShouldReturnLecturerProfile_WhenUserIsLecturer()
        {
            // Arrange
            var foundUser = new User
            {
                UId = 10,
                Email = "abc@gmail.com",
                IsTeacher = true,
                RoleId = RoleConstants.LECTURER,
                Role = new Role
                {
                    RoleId = RoleConstants.LECTURER,
                    RoleName = "LECTURER"
                },
                Lecturer = new Lecturer
                {
                    Fullname = "Lecturer1",
                    Address = "HCM",
                    PhoneNumber = "0123123123",
                    Yob = 1990,
                    AvatarImg = "aaaa",
                    School = "FPT",
                    LecturerCode = "SE123123",
                    Major = "SE"
                }
            };

            _mockUserRepo
                .Setup(r => r.GetOneByUIdWithInclude(It.IsAny<int>()))
                .ReturnsAsync(foundUser);

            var query = new GetUserProfileByIdQuery
            {
                UserId = 10,
                ViewerUId = 99,
                ViewerRole = 1
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.User);
            Assert.Equal("Get profile of User with that id: 10 successfully", result.Message);
        }

        [Fact]
        public async Task GetUserProfileByIdHandler_ShouldReturnStudentProfile_WhenUserIsStudent()
        {
            // Arrange
            var foundUser = new User
            {
                UId = 20,
                Email = "stu@gmail.com",
                IsTeacher = false,
                RoleId = RoleConstants.STUDENT,
                Role = new Role
                {
                    RoleId = RoleConstants.STUDENT,
                    RoleName = "STUDENT"
                },
                Student = new Student
                {
                    Fullname = "Student1",
                    Address = "HCM",
                    PhoneNumber = "0123123123",
                    Yob = 1990,
                    AvatarImg = "aaaa",
                    School = "FPT",
                    StudentCode = "SE123123",
                    Major = "SE"
                }
            };

            _mockUserRepo
                .Setup(r => r.GetOneByUIdWithInclude(It.IsAny<int>()))
                .ReturnsAsync(foundUser);

            var query = new GetUserProfileByIdQuery
            {
                UserId = 20,
                ViewerUId = 99,
                ViewerRole = 1
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.User);
            Assert.Equal("Get profile of User with that id: 20 successfully", result.Message);
        }

        [Fact]
        public async Task GetUserProfileByIdHandler_ShouldReturnFailed_WhenUserNotFound()
        {
            // Arrange
            _mockUserRepo
                .Setup(r => r.GetOneByUIdWithInclude(It.IsAny<int>()))
                .ReturnsAsync((User?)null);

            var query = new GetUserProfileByIdQuery
            {
                UserId = 123,
                ViewerUId = 1,
                ViewerRole = 1
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.User);
        }

        [Fact]
        public async Task GetUserProfileByIdHandler_ShouldCatchException_AndReturnErrorMessage()
        {
            // Arrange
            _mockUserRepo
                .Setup(r => r.GetOneByUIdWithInclude(It.IsAny<int>()))
                .ThrowsAsync(new System.Exception("Database failure"));

            var query = new GetUserProfileByIdQuery { UserId = 1 };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
        }
    }
}
