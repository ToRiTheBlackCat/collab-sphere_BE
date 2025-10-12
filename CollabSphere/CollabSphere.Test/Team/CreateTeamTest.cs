using CollabSphere.Application;
using CollabSphere.Application.Features.Team.Commands;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.Team
{
    public class CreateTeamTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<IStudentRepository> _studentRepoMock;
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<ILecturerRepository> _lecturerRepoMock;
        private readonly Mock<ILogger<CreateTeamHandler>> _loggerMock;
        private readonly CreateTeamHandler _handler;


        public CreateTeamTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepoMock = new Mock<IUserRepository>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _studentRepoMock = new Mock<IStudentRepository>();
            _classRepoMock = new Mock<IClassRepository>();
            _lecturerRepoMock = new Mock<ILecturerRepository>();
            _loggerMock = new Mock<ILogger<CreateTeamHandler>>();

            // Wire repos to unit of work
            _unitOfWorkMock.SetupGet(u => u.UserRepo).Returns(_userRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.StudentRepo).Returns(_studentRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.ClassRepo).Returns(_classRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.LecturerRepo).Returns(_lecturerRepoMock.Object);

            _handler = new CreateTeamHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateTeamHandler_Should_CreateTeam_When_AllValidationPass()
        {
            // Arrange
            var command = new CreateTeamCommand
            {
                TeamName = "Team1",
                LeaderId = 11,
                ClassId = 22,
                LecturerId = 33,
                EnrolKey = null 
            };

            // Mock leader (student)
            _studentRepoMock
                .Setup(s => s.GetStudentById(command.LeaderId))
                .ReturnsAsync(new User
                {
                    UId = command.LeaderId,
                });

            // Mock class (lecturer belongs to class)
            _classRepoMock
                .Setup(c => c.GetById(command.ClassId))
                .ReturnsAsync(new Class
                {
                    ClassId = command.ClassId,
                    LecturerId = command.LecturerId
                });

            // Mock lecturer existence
            _lecturerRepoMock
                .Setup(l => l.GetById(command.LecturerId))
                .ReturnsAsync(new Lecturer
                {
                    LecturerId = command.LecturerId,
                    Fullname = "Lecturer Name"
                });

            // Mock user repo (used for LecturerName mapping)
            _userRepoMock
                .Setup(u => u.GetOneByUIdWithInclude(command.LecturerId))
                .ReturnsAsync(new User
                {
                    UId = command.LecturerId,
                    Lecturer = new Lecturer
                    {
                        LecturerId = command.LecturerId,
                        Fullname = "Lecturer Name"
                    }
                });

            // Capture created team
            Domain.Entities.Team createdTeam = null!;
            _teamRepoMock
                .Setup(t => t.Create(It.IsAny<Domain.Entities.Team>()))
                .Callback<Domain.Entities.Team>(t => createdTeam = t)
                .Returns(Task.CompletedTask);

            // Mock transactions
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Team created successfully.", result.Message);

            _teamRepoMock.Verify(r => r.Create(It.IsAny<Domain.Entities.Team>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);

            Assert.NotNull(createdTeam);
            Assert.Equal(command.TeamName, createdTeam.TeamName);
            Assert.Equal(command.LecturerId, createdTeam.LecturerId);
            Assert.Equal("Lecturer Name", createdTeam.LecturerName);
            Assert.False(string.IsNullOrWhiteSpace(createdTeam.EnrolKey));
            Assert.Equal(6, createdTeam.EnrolKey!.Length);
        }

        [Fact]
        public async Task CreateTeamHandler_ShouldFail_WhenLeaderNotFound()
        {
            // Arrange
            var command = new CreateTeamCommand
            {
                TeamName = "Team1",
                LeaderId = 11,
                ClassId = 22,
                LecturerId = 33
            };

            // Student not found
            _studentRepoMock
                .Setup(s => s.GetStudentById(command.LeaderId))
                .ReturnsAsync((User?)null);

            // Class and lecturer exist (to isolate failure reason)
            _classRepoMock
                .Setup(c => c.GetById(command.ClassId))
                .ReturnsAsync(new Class { ClassId = command.ClassId, LecturerId = command.LecturerId });

            _lecturerRepoMock
                .Setup(l => l.GetById(command.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = command.LecturerId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateTeamHandler_ShouldFail_WhenClassNotFound()
        {
            // Arrange
            var command = new CreateTeamCommand
            {
                TeamName = "Team2",
                LeaderId = 11,
                ClassId = 99,
                LecturerId = 33
            };

            // Student exists
            _studentRepoMock
                .Setup(s => s.GetStudentById(command.LeaderId))
                .ReturnsAsync(new User
                {
                    Student = new Student
                    {
                        StudentId = command.LeaderId
                    }
                });

            // Class not found
            _classRepoMock
                .Setup(c => c.GetById(command.ClassId))
                .ReturnsAsync((Class?)null);

            // Lecturer exists
            _lecturerRepoMock
                .Setup(l => l.GetById(command.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = command.LecturerId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            _teamRepoMock.Verify(r => r.Create(It.IsAny<Domain.Entities.Team>()), Times.Never);
        }

        [Fact]
        public async Task CreateTeamHandler_ShouldFail_WhenLecturerNotBelongToClass()
        {
            // Arrange
            var command = new CreateTeamCommand
            {
                TeamName = "Team3",
                LeaderId = 11,
                ClassId = 22,
                LecturerId = 33
            };

            // Student exists
            _studentRepoMock
                 .Setup(s => s.GetStudentById(command.LeaderId))
                 .ReturnsAsync(new User
                 {
                     Student = new Student
                     {
                         StudentId = command.LeaderId
                     }
                 });

            // Class exists but with a different lecturer
            _classRepoMock
                .Setup(c => c.GetById(command.ClassId))
                .ReturnsAsync(new Class { ClassId = command.ClassId, LecturerId = 999 });

            // Lecturer exists
            _lecturerRepoMock
                .Setup(l => l.GetById(command.LecturerId))
                .ReturnsAsync(new Lecturer { LecturerId = command.LecturerId });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            _teamRepoMock.Verify(r => r.Create(It.IsAny<Domain.Entities.Team>()), Times.Never);
        }

    }
}
