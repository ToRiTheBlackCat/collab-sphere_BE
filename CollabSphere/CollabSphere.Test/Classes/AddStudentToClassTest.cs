using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.Classes.Commands.AddStudent;
using CollabSphere.Application.Features.User.Commands;
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
    public class AddStudentToClassTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IClassRepository> _mockClassRepo;
        private readonly Mock<IClassMemberRepository> _mockClassMemberRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<ILogger<AddStudentToClassHandler>> _logger;
        private readonly AddStudentToClassHandler _handler;

        public AddStudentToClassTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockClassRepo = new Mock<IClassRepository>();
            _mockClassMemberRepo = new Mock<IClassMemberRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _logger = new Mock<ILogger<AddStudentToClassHandler>>();

            //Setup
            // Wire up IUnitOfWork repos
            _unitOfWork.Setup(u => u.ClassRepo).Returns(_mockClassRepo.Object);
            _unitOfWork.Setup(u => u.ClassMemberRepo).Returns(_mockClassMemberRepo.Object);
            _unitOfWork.Setup(u => u.StudentRepo).Returns(_mockStudentRepo.Object);

            _handler = new AddStudentToClassHandler(_unitOfWork.Object, _logger.Object);
        }

        [Fact]
        public async Task AddStudentToClassHandler_ShouldAddStudent_WhenSuccessfully()
        {
            // Arrange
            var command = new AddStudentToClassCommand
            {
                ClassId = 10,
                UserRole = RoleConstants.STAFF,
                StudentList = new List<AddStudentToClass>
                {
                    new AddStudentToClass { StudentId = 1, StudentName = "Test" }
                }
            };

            var existingClass = new Domain.Entities.Class
            {
                ClassId = 10,
                ClassName = "C# Programming"
            };

            var existingStudent = new Student
            {
                StudentId = 1,
                Fullname = "Test"
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            _mockClassRepo.Setup(c => c.GetById(command.ClassId)).ReturnsAsync(existingClass);
            _mockClassMemberRepo.Setup(cm => cm.GetClassMemberAsyncByClassId(command.ClassId)).ReturnsAsync(new List<ClassMember>());
            _mockStudentRepo.Setup(s => s.GetById(1)).ReturnsAsync(existingStudent);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Added student Test to class C# Programming successfully. | Added total 1 students to class with ID: 10 | ", result.Message);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task AddStudentToClassHandler_ShouldReturnMessage_WhenStudentNotFound()
        {
            // Arrange
            var command = new AddStudentToClassCommand
            {
                ClassId = 20,
                UserRole = RoleConstants.STAFF,
                StudentList = new List<AddStudentToClass>
                {
                    new AddStudentToClass { StudentId = 2, StudentName = "Missing" }
                }
            };

            var existingClass = new Class { ClassId = 20, ClassName = "Math" };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockClassRepo.Setup(c => c.GetById(command.ClassId)).ReturnsAsync(existingClass);
            _mockClassMemberRepo.Setup(cm => cm.GetClassMemberAsyncByClassId(command.ClassId)).ReturnsAsync(new List<ClassMember>());
            _mockStudentRepo.Setup(s => s.GetById(2)).ReturnsAsync((Student?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Not found any student with Id: 2", result.Message);
        }

        [Fact]
        public async Task AddStudentToClassHandler_ShouldSkip_WhenStudentAlreadyInClass()
        {
            // Arrange
            var command = new AddStudentToClassCommand
            {
                ClassId = 5,
                UserRole = RoleConstants.STAFF,
                StudentList = new List<AddStudentToClass>
                {
                    new AddStudentToClass { StudentId = 3, StudentName = "Test" }
                }
            };

            var existingClass = new Class { ClassId = 5, ClassName = "English" };
            var existingStudent = new Student { StudentId = 3, Fullname = "Test" };
            var classMembers = new List<ClassMember>
            {
                new ClassMember { StudentId = 3, ClassId = 5 }
            };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockClassRepo.Setup(c => c.GetById(command.ClassId)).ReturnsAsync(existingClass);
            _mockStudentRepo.Setup(s => s.GetById(3)).ReturnsAsync(existingStudent);
            _mockClassMemberRepo.Setup(cm => cm.GetClassMemberAsyncByClassId(5)).ReturnsAsync(classMembers);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Student Test already in class English. Cannot add this student to class | ", result.Message);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task AddStudentToClassHandler_ShouldSkip_WhenStudentNameNotMatch()
        {
            // Arrange
            var command = new AddStudentToClassCommand
            {
                ClassId = 15,
                UserRole = RoleConstants.STAFF,
                StudentList = new List<AddStudentToClass>
                {
                    new AddStudentToClass { StudentId = 4, StudentName = "Wrong Name" }
                }
            };

            var existingClass = new Class { ClassId = 15, ClassName = "Physics" };
            var existingStudent = new Student { StudentId = 4, Fullname = "Correct Name" };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _mockClassRepo.Setup(c => c.GetById(command.ClassId)).ReturnsAsync(existingClass);
            _mockStudentRepo.Setup(s => s.GetById(4)).ReturnsAsync(existingStudent);
            _mockClassMemberRepo.Setup(cm => cm.GetClassMemberAsyncByClassId(command.ClassId)).ReturnsAsync(new List<ClassMember>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("Student name Wrong Name does not match with existing student name Correct Name. Cannot add this student to class | ", result.Message);
        }

        [Fact]
        public async Task AddStudentToClassHandler_ShouldRollback_WhenExceptionOccur()
        {
            // Arrange
            var command = new AddStudentToClassCommand
            {
                ClassId = 100,
                UserRole = RoleConstants.STAFF,
                StudentList = new List<AddStudentToClass>
                {
                    new AddStudentToClass { StudentId = 5, StudentName = "Crash" }
                }
            };

            var existingClass = new Class { ClassId = 100, ClassName = "Error Simulation" };
            var existingStudent = new Student { StudentId = 5, Fullname = "Crash" };

            _unitOfWork.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

            _mockClassRepo.Setup(c => c.GetById(command.ClassId)).ReturnsAsync(existingClass);
            _mockClassMemberRepo.Setup(cm => cm.GetClassMemberAsyncByClassId(command.ClassId))
                .ReturnsAsync(new List<ClassMember>());
            _mockStudentRepo.Setup(s => s.GetById(5)).ReturnsAsync(existingStudent);

            _unitOfWork.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new Exception("DB Save Failed"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("An error occurred while adding student to class", result.Message);
            _unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            _unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }
    }
}
