using CollabSphere.Application;
using CollabSphere.Application.DTOs.Classes;
using CollabSphere.Application.Features.Classes.Commands.UpdateClass;
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
    public class UpdateClassHandlerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ISubjectRepository> _subjectRepo;
        private readonly Mock<ILecturerRepository> _lecturerRepo;
        private readonly Mock<IClassRepository> _classRepo;

        private readonly UpdateClassHandler _handler;

        public UpdateClassHandlerTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _subjectRepo = new Mock<ISubjectRepository>();
            _lecturerRepo = new Mock<ILecturerRepository>();
            _classRepo = new Mock<IClassRepository>();

            _unitOfWork.Setup(x => x.SubjectRepo).Returns(_subjectRepo.Object);
            _unitOfWork.Setup(x => x.LecturerRepo).Returns(_lecturerRepo.Object);
            _unitOfWork.Setup(x => x.ClassRepo).Returns(_classRepo.Object);

            _handler = new UpdateClassHandler(_unitOfWork.Object);
        }

        private UpdateClassCommand GenerateValidCommand()
        {
            return new UpdateClassCommand()
            {
                ClassDto = new Application.DTOs.Classes.UpdateClassDto()
                {
                    ClassId = 1,
                    ClassName = "Updated ClassName",
                    EnrolKey = "12345",
                    LecturerId = 8,
                    SubjectId = 11,
                    IsActive = true,
                }
            }; ;
        }

        [Fact]
        public async Task Handle_ShouldUpdateProject_WhenValidRequest()
        {
            // Arrange
            var command = GenerateValidCommand();

            var lecturer = new Lecturer()
            {
                LecturerId = 8,
                Fullname = "Lecturer8",
            };

            var subject = new Subject()
            {
                SubjectId = 11,
                SubjectName = "Subject11",
            };
            _classRepo.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, ClassName = "ClassName", LecturerId = 2, SubjectId = 9 ,IsActive = false, EnrolKey = "None" });
            _subjectRepo.Setup(x => x.GetById(11)).ReturnsAsync(subject);
            _lecturerRepo.Setup(x => x.GetById(8)).ReturnsAsync(lecturer);

            var capturedClass = new Class();
            _classRepo.Setup(x => x.Update(It.IsAny<Class>())).Callback<Class>(x => capturedClass = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);

            _classRepo.Verify(x => x.Update(It.IsAny<Class>()), Times.Once);
            Assert.Equal("Updated ClassName", capturedClass.ClassName);
            Assert.Equal("12345", capturedClass.EnrolKey);
            Assert.Equal(8, capturedClass.LecturerId);
            Assert.Equal("Lecturer8", capturedClass.LecturerName);
            Assert.Equal(11, capturedClass.SubjectId);
            Assert.True(capturedClass.IsActive);
        }

        [Fact]
        public async Task Handle_ShouldUpdateProject_WhenOnlyEmptyFields()
        {
            // Arrange
            var command = GenerateValidCommand();
            var updateDto = new UpdateClassDto()
            {
                ClassId = 1,
                ClassName = "NameOnly",
            };
            command.ClassDto = updateDto;

            _classRepo.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, ClassName = "ClassName", LecturerId = 2, SubjectId = 9, IsActive = false, EnrolKey = "None" });

            var capturedClass = new Class();
            _classRepo.Setup(x => x.Update(It.IsAny<Class>())).Callback<Class>(x => capturedClass = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);

            _classRepo.Verify(x => x.Update(It.IsAny<Class>()), Times.Once);
            Assert.Equal("NameOnly", capturedClass.ClassName);
            Assert.Equal("None", capturedClass.EnrolKey);
            Assert.Equal(2, capturedClass.LecturerId);
            Assert.Equal(9, capturedClass.SubjectId);
            Assert.False(capturedClass.IsActive);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNoSubjectFound()
        {
            // Arrange
            var command = GenerateValidCommand();

            var lecturer = new Lecturer()
            {
                LecturerId = 8,
                Fullname = "Lecturer8",
            };

            var subject = new Subject()
            {
                SubjectId = 11,
                SubjectName = "Subject11",
            };
            _classRepo.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, ClassName = "ClassName", LecturerId = 2, SubjectId = 9, IsActive = false, EnrolKey = "None" });
            //_subjectRepo.Setup(x => x.GetById(11)).ReturnsAsync(subject);
            _lecturerRepo.Setup(x => x.GetById(8)).ReturnsAsync(lecturer);

            var capturedClass = new Class();
            _classRepo.Setup(x => x.Update(It.IsAny<Class>())).Callback<Class>(x => capturedClass = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No subject with ID: 11", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNoLecturerFound()
        {
            // Arrange
            var command = GenerateValidCommand();

            var lecturer = new Lecturer()
            {
                LecturerId = 8,
                Fullname = "Lecturer8",
            };

            var subject = new Subject()
            {
                SubjectId = 11,
                SubjectName = "Subject11",
            };
            _classRepo.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, ClassName = "ClassName", LecturerId = 2, SubjectId = 9, IsActive = false, EnrolKey = "None" });
            _subjectRepo.Setup(x => x.GetById(11)).ReturnsAsync(subject);
            //_lecturerRepo.Setup(x => x.GetById(8)).ReturnsAsync(lecturer);

            var capturedClass = new Class();
            _classRepo.Setup(x => x.Update(It.IsAny<Class>())).Callback<Class>(x => capturedClass = x);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("No lecturer with ID: 8", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = GenerateValidCommand();

            var lecturer = new Lecturer()
            {
                LecturerId = 8,
                Fullname = "Lecturer8",
            };

            var subject = new Subject()
            {
                SubjectId = 11,
                SubjectName = "Subject11",
            };
            _classRepo.Setup(x => x.GetById(1)).ReturnsAsync(new Domain.Entities.Class() { ClassId = 1, ClassName = "ClassName", LecturerId = 2, SubjectId = 9, IsActive = false, EnrolKey = "None" });
            _subjectRepo.Setup(x => x.GetById(11)).ReturnsAsync(subject);
            _lecturerRepo.Setup(x => x.GetById(8)).ReturnsAsync(lecturer);

            var capturedClass = new Class();
            _classRepo.Setup(x => x.Update(It.IsAny<Class>())).Callback<Class>(x => capturedClass = x);
            _classRepo.Setup(x => x.Update(It.IsAny<Class>())).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Contains("DB Exception", result.Message);
        }
    }
}
