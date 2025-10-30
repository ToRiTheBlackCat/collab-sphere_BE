using CollabSphere.Application;
using CollabSphere.Application.Constants;
using CollabSphere.Application.Features.TeamMilestones.Commands.CheckTeamMilestone;
using CollabSphere.Application.Features.TeamMilestones.Commands.CreateCustomTeamMilestone;
using CollabSphere.Domain.Entities;
using CollabSphere.Domain.Intefaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabSphere.Test.TeamMilestones
{
    public class CreateTeamMilestoneTest
    {

        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITeamRepository> _teamRepoMock;
        private readonly Mock<ITeamMilestoneRepository> _teamMilestoneRepoMock;

        private readonly CreateCustomTeamMilestoneHandler _handler;

        public CreateTeamMilestoneTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _teamRepoMock = new Mock<ITeamRepository>();
            _teamMilestoneRepoMock = new Mock<ITeamMilestoneRepository>();

            _unitOfWorkMock.Setup(x => x.TeamRepo).Returns(_teamRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.TeamMilestoneRepo).Returns(_teamMilestoneRepoMock.Object);

            _handler = new CreateCustomTeamMilestoneHandler(_unitOfWorkMock.Object);
        }

        public void SetupMocks()
        {
            var team = new Domain.Entities.Team()
            {
                TeamId = 11,
                Class = new Class()
                {
                    ClassId = 1,
                    LecturerId = 88,
                },
                ClassMembers = new List<ClassMember>()
                {
                    new ClassMember()
                    {
                        ClassMemberId = 33,
                        StudentId = 3,
                        TeamRole = (int)TeamRole.LEADER
                    }
                },
            };

            _teamRepoMock.Setup(x => x.GetTeamDetail(11)).ReturnsAsync(team);
        }

        [Fact]
        public async Task Handle_ShouldCreateTeamMilestone_WhenValidCommand()
        {
            // Arrange
            var command = new CreateCustomTeamMilestoneCommand()
            {
                MilestoneDto = new Application.DTOs.TeamMilestones.CreateTeamMilestoneDto()
                {
                    TeamId = 11,
                    Title = "Test title",
                    Description = "Test desc",
                    StartDate = new DateOnly(2025, 10, 15),
                    EndDate = new DateOnly(2025, 10, 19),
                },
                UserId = 88,
                UserRole = (int)RoleConstants.LECTURER
            };

            this.SetupMocks();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.True(result.IsSuccess);
            _teamMilestoneRepoMock.Verify(x =>
                x.Create(It.Is<TeamMilestone>(x => 
                    x.Title == "Test title" &&
                    x.Description == "Test desc" &&
                    x.TeamId == 11
                )),
                Times.Once
            );
            _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRollBack_WhenDBException()
        {
            // Arrange
            var command = new CreateCustomTeamMilestoneCommand()
            {
                MilestoneDto = new Application.DTOs.TeamMilestones.CreateTeamMilestoneDto()
                {
                    TeamId = 11,
                    Title = "Test title",
                    Description = "Test desc",
                    StartDate = new DateOnly(2025, 10, 15),
                    EndDate = new DateOnly(2025, 10, 19),
                },
                UserId = 88,
                UserRole = (int)RoleConstants.LECTURER
            };

            this.SetupMocks();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Empty(result.ErrorList);
            Assert.Contains("DB Exception", result.Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenNotLecturerOfClass()
        {
            // Arrange
            var command = new CreateCustomTeamMilestoneCommand()
            {
                MilestoneDto = new Application.DTOs.TeamMilestones.CreateTeamMilestoneDto()
                {
                    TeamId = 11,
                    Title = "Test title",
                    Description = "Test desc",
                    StartDate = new DateOnly(2025, 10, 15),
                    EndDate = new DateOnly(2025, 10, 19),
                },
                UserId = 77,
                UserRole = (int)RoleConstants.LECTURER
            };

            this.SetupMocks();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("the assigned lecturer of the class", result.ErrorList.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldFailValidation_WhenIsStudent()
        {
            // Arrange
            var command = new CreateCustomTeamMilestoneCommand()
            {
                MilestoneDto = new Application.DTOs.TeamMilestones.CreateTeamMilestoneDto()
                {
                    TeamId = 11,
                    Title = "Test title",
                    Description = "Test desc",
                    StartDate = new DateOnly(2025, 10, 15),
                    EndDate = new DateOnly(2025, 10, 19),
                },
                UserId = 33,
                UserRole = (int)RoleConstants.STUDENT
            };

            this.SetupMocks();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Throws(new Exception("DB Exception"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsValidInput);
            Assert.False(result.IsSuccess);
            Assert.Single(result.ErrorList);
            Assert.Contains("the assigned lecturer of the class", result.ErrorList.First().Message);
        }
    }
}
