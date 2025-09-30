using Castle.Core.Configuration;
using Castle.Core.Logging;
using CollabSphere.Application;
using CollabSphere.Application.DTOs.User;
using CollabSphere.Application.Features.Auth.Commands;
using CollabSphere.Application.Features.Classes.Commands.CreateClass;
using CollabSphere.Application.Features.User.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace CollabSphere.Test.HeadDepartment_Staff
{
    public class CreateHeadDepartment_StaffAccTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly HeadDepart_StaffSignUpSignUpHandler _handler;
        private readonly Mock<IConfiguration> _configure;
        private readonly Mock<ILogger<HeadDepart_StaffSignUpSignUpHandler>> _logger;
        public CreateHeadDepartment_StaffAccTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();
            _configure = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<HeadDepart_StaffSignUpSignUpHandler>>();
            _unitOfWork.Setup(u => u.UserRepo).Returns(_mockUserRepo.Object);


            _handler = new HeadDepart_StaffSignUpSignUpHandler(_unitOfWork.Object, _configure.Object, _logger.Object);
        }

        [Fact]
        public async Task CreateHeadDepartmentHandler_ShouldReturnSuccess_WhenValid()
        {
            //Arrange

            _mockUserRepo.Setup(r => r.GetOneByEmail(It.IsAny<string>()))
                         .ReturnsAsync((User?)null);

            var command = new HeadDepart_StaffSignUpCommand(new HeadDepart_StaffSignUpRequestDto
            {
                Email = "FakeHeadDepart1@gmail.com",
                Password = "123",
                isStaff = false
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);
            //Assert
            Assert.True(result.Item1);
        }

        [Fact]
        public async Task CreateStaffHandler_ShouldReturnSuccess_WhenValid()
        {
            //Arrange

            _mockUserRepo.Setup(r => r.GetOneByEmail(It.IsAny<string>()))
                         .ReturnsAsync((User?)null);

            var command = new HeadDepart_StaffSignUpCommand(new HeadDepart_StaffSignUpRequestDto
            {
                Email = "Staff1@gmail.com",
                Password = "123",
                isStaff = true
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);
            //Assert
            Assert.True(result.Item1);
        }

        [Fact]
        public async Task CreateHandler_ShouldReturnExisted_WhenFoundUserWithEmail()
        {
            //Arrange
            var existedUser = new User
            {
                Email = "FoundUser@gmail.com",
                Password = "123"
            };

            _mockUserRepo.Setup(r => r.GetOneByEmail(It.IsAny<string>()))
                         .ReturnsAsync(existedUser);

            var command = new HeadDepart_StaffSignUpCommand(new HeadDepart_StaffSignUpRequestDto
            {
                Email = "FoundUser@gmail.com",
                Password = "123",
                isStaff = true
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);
            //Assert
            Assert.False(result.Item1);
            Assert.Equivalent(result.Item2,"Already exist staff or head department with that email. Try other email to create account");
        }

        [Fact]
        public async Task CreateHandler_ShouldReturnException_WhenExceptionWhenCreate()
        {
            //Arrange
            _mockUserRepo.Setup(r => r.GetOneByEmail(It.IsAny<string>()))
                         .ReturnsAsync((User?)null);
            _mockUserRepo.Setup(r => r.InsertUser(It.IsAny<User>()))
                .Throws(new Exception());

            var command = new HeadDepart_StaffSignUpCommand(new HeadDepart_StaffSignUpRequestDto
            {
                Email = "FoundUser@gmail.com",
                Password = "123",
                isStaff = true
            });

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);
            //Assert
            Assert.False(result.Item1);
            Assert.Equivalent(result.Item2, "Exception when create new staff or head department");
        }
    }
}
