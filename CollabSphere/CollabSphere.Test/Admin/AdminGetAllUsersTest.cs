using CollabSphere.Application;
using CollabSphere.Application.Common;
using CollabSphere.Application.Features.Admin.Queries;
using CollabSphere.Application.Features.Token.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Admin
{
    public class AdminGetAllUsersTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ILogger<AdminGetAllUsersHandler>> _mockLogger;

        private readonly AdminGetAllUsersHandler _handler;

        public AdminGetAllUsersTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<AdminGetAllUsersHandler>>();
            _handler = new AdminGetAllUsersHandler(_mockUnitOfWork.Object, _mockLogger.Object);
            //Set up configure UnitOfWork and Repo
            _mockUnitOfWork.Setup(c => c.UserRepo).Returns(_mockUserRepo.Object);
        }
 
        [Fact]
        public async Task AdminGetAllUsersHandler_ShouldReturnCounts_WhenUsersExist()
        {
            // Arrange
            var headDep = new List<User> { new User { Email = "head@gmail.com", RoleId = 2 } };
            var staff = new List<User> { new User { Email = "staff@gmail.com", RoleId = 3 } };
            var lecturers = new List<User> { new User { Email = "lect@gmail.com", RoleId = 4 } };
            var students = new List<User> { new User { Email = "stud@gmail.com", RoleId = 5 } };

            _mockUserRepo.Setup(r => r.GetAllHeadDepartAsync()).ReturnsAsync(headDep);
            _mockUserRepo.Setup(r => r.GetAllStaffAsync()).ReturnsAsync(staff);
            _mockUserRepo.Setup(r => r.GetAllLecturerAsync()).ReturnsAsync(lecturers);
            _mockUserRepo.Setup(r => r.GetAllStudentAsync()).ReturnsAsync(students);

            // Act
            var result = await _handler.Handle(new AdminGetAllUsersQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(1, result.HeadDepartmentCount);
            Assert.Equal(1, result.StaffCount);
            Assert.Equal(1, result.LecturerCount);
            Assert.Equal(1, result.StudentCount);
        }
    }
}
